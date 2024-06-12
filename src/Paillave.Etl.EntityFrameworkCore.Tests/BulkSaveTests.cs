using Paillave.Etl.EntityFrameworkCore.Tests.Entities;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.EntityFrameworkCore.Tests
{
    public class BulkSaveTests : SqlDatabaseTests
    {
        public BulkSaveTests(MsSqlDatabaseFixture fixture) : base(fixture)
        { }

        [Fact]
        public async Task GivenTableWithoutIdentityColumn_WhenEfCoreSave_ThenItemsSaved()
        {
            // arange
            var executionOptions = new ExecutionOptions<int>
            {
                Resolver = new SimpleDependencyResolver()
                    .Register<DbContext>(Context)
            };

            var streamProcessRunner = StreamProcessRunner.Create((ISingleStream<int> configStream) => configStream
                .CrossApply("generate ids", i => Enumerable.Range(1, i))
                .EfCoreSave("insert", builder => builder.Entity(i => new Post()
                {
                    PostId = i,
                    Name = $"Post {i}",
                }).InsertOnly()), "GivenTableWithoutIdentityColumn_WhenEfCoreSave_ThenItemsSaved");


            // act
            await streamProcessRunner.ExecuteAsync(3, executionOptions);


            //assert
            var posts = await Context.Posts.ToListAsync();
            posts.Should().BeEquivalentTo(new[]
            {
                new Post() { PostId = 1, Name = "Post 1", },
                new Post() { PostId = 2, Name = "Post 2", },
                new Post() { PostId = 3, Name = "Post 3", }
            });
        }

        [Fact]
        public async Task GivenTableWithIdentityColumn_WhenEfCoreSave_ThenItemsSaved()
        {
            // arange
            var executionOptions = new ExecutionOptions<int>
            {
                Resolver = new SimpleDependencyResolver()
                    .Register<DbContext>(Context)
            };

            var streamProcessRunner = StreamProcessRunner.Create((ISingleStream<int> configStream) => configStream
                    .CrossApply("generate ids", i => Enumerable.Range(1, i))
                    .EfCoreSave("insert", builder => builder.Entity(i => new Blog()
                    {
                        BlogId = i,
                        Name = $"Blog {i}",
                    }).InsertOnly().WithIdentityInsert()), "GivenTableWithIdentityColumn_WhenEfCoreSave_ThenItemsSaved");


            // act
            await streamProcessRunner.ExecuteAsync(3, executionOptions);


            //assert
            var blogs = await Context.Blogs.ToListAsync();
            blogs.Should().BeEquivalentTo(new[]
                {
                    new Blog() { BlogId = 1, Name = "Blog 1", },
                    new Blog() { BlogId = 2, Name = "Blog 2", },
                    new Blog() { BlogId = 3, Name = "Blog 3", }
                });
        }
    }
}