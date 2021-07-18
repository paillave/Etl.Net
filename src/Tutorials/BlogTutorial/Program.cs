using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.TextFile;
using Paillave.Etl.EntityFrameworkCore;
using BlogTutorial.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BlogTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            processRunner.DebugNodeStream += (sender, e) 
                => { /* PLACE A CONDITIONAL BREAKPOINT HERE FOR DEBUG ex: e.NodeName == "parse file" */ };
            using (var dbCtx = new SimpleTutorialDbContext(args[1]))
            {
                var executionOptions = new ExecutionOptions<string> { 
                    Resolver = new SimpleDependencyResolver().Register<DbContext>(dbCtx) };
                var res = await processRunner.ExecuteAsync(args[0], executionOptions);
                Console.Write(res.Failed ? "Failed" : "Succeeded");
            }
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            var rowStream = contextStream
              .CrossApplyFolderFiles("list all required files", "*.csv", true)
              .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new
              {
                  Author = i.ToColumn("author"),
                  Email = i.ToColumn("email"),
                  TimeSpan = i.ToDateColumn("timestamp", "yyyyMMddHHmmss"),
                  Category = i.ToColumn("category"),
                  Link = i.ToColumn("link"),
                  Post = i.ToColumn("post"),
                  Title = i.ToColumn("title"),
              }).IsColumnSeparated(','))
              .SetForCorrelation("set correlation for row");

            var authorStream = rowStream
                .Distinct("remove author duplicates based on emails", i => i.Email)
                .Select("create author instance", i => new Author { Email = i.Email, Name = i.Author })
                .EfCoreSaveCorrelated("save authors", o => o.SeekOn(i => i.Email).AlternativelySeekOn(i => i.Name));

            var categoryStream = rowStream
                .Distinct("remove category duplicates", i => i.Category)
                .Select("create category instance", i => new Category { Code = i.Category, Name = i.Category })
                .EfCoreSaveCorrelated("save categories", o => o.SeekOn(i => i.Code).DoNotUpdateIfExists());

            var postStream = rowStream
                .CorrelateToSingle("get related category", categoryStream, (l, r) => new { Row = l, Category = r })
                .CorrelateToSingle("get related author", authorStream, (l, r) => new { l.Row, l.Category, Author = r })
                .Select("create post instance", i => string.IsNullOrWhiteSpace(i.Row.Post)
                    ? new LinkPost
                    {
                        AuthorId = i.Author.Id,
                        CategoryId = i.Category.Id,
                        DateTime = i.Row.TimeSpan,
                        Title = i.Row.Title,
                        Url = new Uri(i.Row.Link)
                    } as Post
                    : new TextPost
                    {
                        AuthorId = i.Author.Id,
                        CategoryId = i.Category.Id,
                        DateTime = i.Row.TimeSpan,
                        Title = i.Row.Title,
                        Text = i.Row.Post
                    })
                .EfCoreSaveCorrelated("save posts", o => o.SeekOn(i => new { i.AuthorId, i.DateTime }));
        }
    }
}
