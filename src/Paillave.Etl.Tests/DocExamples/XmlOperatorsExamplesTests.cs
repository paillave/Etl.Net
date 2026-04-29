using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.XmlFile;
using Xunit;

namespace Paillave.Etl.Tests.DocExamples;

// Examples mirrored by `documentation/docs/operators/10_xml.md`.
public class XmlOperatorsExamplesTests
{
    public sealed class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
    }

    public sealed class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public sealed class Pet
    {
        public string Name { get; set; } = "";
    }

    private static IFileValue Xml(string content, string name = "data.xml")
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return new InMemoryFileValue(ms, name);
    }

    // ===================================================================
    // Parse a single node type with attributes + child elements
    // ===================================================================

    [Fact]
    public async Task XmlNodeOfType_ParsesAttributesAndChildren()
    {
        var xml = Xml("""
            <?xml version="1.0" encoding="UTF-8"?>
            <root>
              <person id="1" firstName="John" lastName="Doe"><age>30</age></person>
              <person id="2" firstName="Jane" lastName="Roe"><age>27</age></person>
            </root>
            """);

        var people = new ConcurrentBag<Person>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            xml,
            root => root
                .CrossApply("source", _ => new[] { xml })
                .CrossApplyXmlFile("parse", d => d.AddNodeDefinition<Person>(
                    "person",
                    "/root/person",
                    m => new Person
                    {
                        Id        = m.ToXPathQuery<int>("/root/person/@id"),
                        FirstName = m.ToXPathQuery<string>("/root/person/@firstName"),
                        LastName  = m.ToXPathQuery<string>("/root/person/@lastName"),
                        Age       = m.ToXPathQuery<int>("/root/person/age"),
                    }))
                .XmlNodeOfType<Person>("to person", "person")
                .Do("collect", p => people.Add(p)));

        Assert.False(status.Failed);
        Assert.Equal(2, people.Count);
        Assert.Contains(people, p => p.Id == 1 && p.FirstName == "John" && p.Age == 30);
        Assert.Contains(people, p => p.Id == 2 && p.FirstName == "Jane" && p.Age == 27);
    }

    // ===================================================================
    // Parse multiple node types from the same document
    // ===================================================================

    [Fact]
    public async Task XmlNodeOfType_ParsesHeterogeneousNodes()
    {
        var xml = Xml("""
            <?xml version="1.0" encoding="UTF-8"?>
            <root>
              <person id="1" firstName="John" lastName="Doe"><age>30</age></person>
              <company id="42" name="Contoso" />
            </root>
            """);

        var people    = new ConcurrentBag<Person>();
        var companies = new ConcurrentBag<Company>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            xml,
            root =>
            {
                var parsed = root
                    .CrossApply("source", _ => new[] { xml })
                    .CrossApplyXmlFile("parse", d => d
                        .AddNodeDefinition<Person>("person", "/root/person",
                            m => new Person
                            {
                                Id        = m.ToXPathQuery<int>("/root/person/@id"),
                                FirstName = m.ToXPathQuery<string>("/root/person/@firstName"),
                                LastName  = m.ToXPathQuery<string>("/root/person/@lastName"),
                                Age       = m.ToXPathQuery<int>("/root/person/age"),
                            })
                        .AddNodeDefinition<Company>("company", "/root/company",
                            m => new Company
                            {
                                Id   = m.ToXPathQuery<int>("/root/company/@id"),
                                Name = m.ToXPathQuery<string>("/root/company/@name"),
                            }));
                parsed.XmlNodeOfType<Person>("p",  "person").Do("p+",  p => people.Add(p));
                parsed.XmlNodeOfType<Company>("c", "company").Do("c+", c => companies.Add(c));
            });

        Assert.False(status.Failed);
        Assert.Single(people);
        Assert.Single(companies);
        Assert.Equal("Contoso", companies.First().Name);
    }

    // ===================================================================
    // Correlated parsing — group child rows under their parent
    // ===================================================================

    [Fact]
    public async Task XmlNodeOfTypeCorrelated_GroupsChildrenUnderParent()
    {
        var xml = Xml("""
            <?xml version="1.0" encoding="UTF-8"?>
            <root>
              <person id="1" firstName="John">
                <pet name="Rex" />
                <pet name="Whiskers" />
              </person>
              <person id="2" firstName="Jane">
                <pet name="Goldie" />
              </person>
            </root>
            """);

        var pets = new ConcurrentBag<Correlated<Pet>>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            xml,
            root => root
                .CrossApply("source", _ => new[] { xml })
                .CrossApplyXmlFile("parse", d => d
                    .AddNodeDefinition<Person>("person", "/root/person",
                        m => new Person
                        {
                            Id        = m.ToXPathQuery<int>("/root/person/@id"),
                            FirstName = m.ToXPathQuery<string>("/root/person/@firstName"),
                        })
                    .AddNodeDefinition<Pet>("pet", "/root/person/pet",
                        m => new Pet
                        {
                            Name = m.ToXPathQuery<string>("/root/person/pet/@name"),
                        }))
                .XmlNodeOfTypeCorrelated<Pet>("pets", "/root/person", "pet")
                .Do("collect", p => pets.Add(p)));

        Assert.False(status.Failed);
        Assert.Equal(3, pets.Count);
        // each pet carries the correlation keys of every ancestor it was matched against
        Assert.All(pets, p => Assert.NotEmpty(p.CorrelationKeys));
    }
}
