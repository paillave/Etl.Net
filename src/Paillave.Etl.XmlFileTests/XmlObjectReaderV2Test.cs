using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Paillave.Etl.XmlFile.Core;
using Xunit;

namespace Paillave.Etl.XmlFileTests;

public class XmlObjectReaderV2Tests
{
    // [Fact]
    // public void Read_SimpleXmlWithAttributes_ParsesCorrectly3()
    // {
    //     // Arrange
    //     var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
    //                    <root rootId=""a root id"">
    //                      <data>root data</data>
    //                      <person id=""1"" firstName=""John"" lastName=""Doe"">
    //                        <age>30</age>
    //                         <extraFields>
    //                          <field name=""street"">123 Main St</field>
    //                          <field name=""city"">Springfield</field>
    //                         </extraFields>
    //                      </person>
    //                      <person id=""2"" firstName=""Coucou"">
    //                        <age>32</age>
    //                      </person>
    //                      <company id=""3"" name=""MyCompany"">
    //                        <age>32</age>
    //                         <extraFields>
    //                          <field name=""city"">Big City</field>
    //                         </extraFields>
    //                      </company>
    //                    </root>";

    //     var definition = new XmlFileDefinition();
    //     definition.AddNodeDefinition(
    //         "person",
    //         "/root/person",
    //         i => new TestPerson
    //         {
    //             Id = i.ToXPathQuery<int>("/root/person/@id"),
    //             FirstName = i.ToXPathQuery<string>("/root/person/@firstName"),
    //             LastName = i.ToXPathQuery<string>("/root/person/@lastName"),
    //             Age = i.ToXPathQuery<int>("/root/person/age"),
    //             Street = i.ToXPathQuery<string>(@"/root/person/extraFields/field[@name=""street""]"),
    //             City = i.ToXPathQuery<string>(@"/root/person/extraFields/field[@name=""city""]"),
    //             RootId = i.ToXPathQuery<string>("/root/@rootId"),
    //             RootData = i.ToXPathQuery<string>("/root/data"),
    //         });
    //     definition.AddNodeDefinition(
    //         "company",
    //         "/root/company",
    //         i => new TestCompany
    //         {
    //             Id = i.ToXPathQuery<int>("/root/company/@id"),
    //             Name = i.ToXPathQuery<string>("/root/company/@name"),
    //             Street = i.ToXPathQuery<string>(@"/root/company/extraFields/field[@name=""street""]"),
    //             City = i.ToXPathQuery<string>(@"/root/company/extraFields/field[@name=""city""]"),
    //             RootId = i.ToXPathQuery<string>("/root/@rootId"),
    //             RootData = i.ToXPathQuery<string>("/root/data"),
    //         });

    //     var results = new List<XmlNodeParsed>();
    //     var reader = new XmlObjectReaderV2(definition, "test", results.Add);

    //     // Act
    //     using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
    //     {
    //         reader.Read(stream, CancellationToken.None);
    //     }

    //     var people = results.FindAll(i => i.NodeDefinitionName == "person").ToList();
    //     var person1 = people[0].Value as TestPerson;
    //     Assert.Equal(1, person1.Id);
    //     Assert.Equal("John", person1.FirstName);
    //     Assert.Equal("Doe", person1.LastName);
    //     Assert.Equal(30, person1.Age);
    //     Assert.Equal("123 Main St", person1.Street);
    //     Assert.Equal("Springfield", person1.City);
    //     Assert.Equal("a root id", person1.RootId);
    //     Assert.Equal("root data", person1.RootData);
    //     var person2 = results[1].Value as TestPerson;
    //     Assert.Equal(2, person2.Id);
    //     Assert.Equal("Coucou", person2.FirstName);
    //     Assert.Null(person2.LastName);
    //     Assert.Equal(32, person2.Age);
    //     Assert.Null(person2.Street);
    //     Assert.Null(person2.City);
    //     Assert.Equal("a root id", person2.RootId);
    //     Assert.Equal("root data", person2.RootData);

    //     var companies = results.FindAll(i => i.NodeDefinitionName == "company").ToList();
    //     var company1 = companies[0].Value as TestCompany;
    //     Assert.Equal(3, company1.Id);
    //     Assert.Equal("MyCompany", company1.Name);
    //     Assert.Null(company1.Street);
    //     Assert.Equal("Big City", company1.City);
    //     Assert.Equal("a root id", company1.RootId);
    //     Assert.Equal("root data", company1.RootData);
    // }

    [Fact]
    public void Read_SimpleXmlWithAttributes_ParsesCorrectly2()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                       <root rootId=""a root id"">
                         <data>root data</data>
                         <person id=""1"" firstName=""John"" lastName=""Doe"">
                           <age>30</age>
                           <address>
                             <street>123 Main St</street>
                             <city>Springfield</city>
                            </address>
                         </person>
                         <person id=""2"" firstName=""Coucou"">
                           <age>32</age>
                         </person>
                         <company id=""3"" name=""MyCompany"">
                           <age>32</age>
                           <address>
                             <city>Big City</city>
                            </address>
                         </company>
                       </root>";

        var definition = new XmlFileDefinition();
        definition.AddNodeDefinition(
            "person",
            "/root/person",
            i => new TestPerson
            {
                Id = i.ToXPathQuery<int>("/root/person/@id"),
                FirstName = i.ToXPathQuery<string>("/root/person/@firstName"),
                LastName = i.ToXPathQuery<string>("/root/person/@lastName"),
                Age = i.ToXPathQuery<int>("/root/person/age"),
                Street = i.ToXPathQuery<string>("/root/person/address/street"),
                City = i.ToXPathQuery<string>("/root/person/address/city"),
                RootId = i.ToXPathQuery<string>("/root/@rootId"),
                RootData = i.ToXPathQuery<string>("/root/data"),
            });
        definition.AddNodeDefinition(
            "company",
            "/root/company",
            i => new TestCompany
            {
                Id = i.ToXPathQuery<int>("/root/company/@id"),
                Name = i.ToXPathQuery<string>("/root/company/@name"),
                Street = i.ToXPathQuery<string>("/root/company/address/street"),
                City = i.ToXPathQuery<string>("/root/company/address/city"),
                RootId = i.ToXPathQuery<string>("/root/@rootId"),
                RootData = i.ToXPathQuery<string>("/root/data"),
            });

        var results = new List<XmlNodeParsed>();
        var reader = new XmlObjectReaderV2(definition, "test", results.Add);

        // Act
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
        {
            reader.Read(stream, CancellationToken.None);
        }

        var people = results.FindAll(i => i.NodeDefinitionName == "person").ToList();
        var person1 = people[0].Value as TestPerson;
        Assert.Equal(1, person1.Id);
        Assert.Equal("John", person1.FirstName);
        Assert.Equal("Doe", person1.LastName);
        Assert.Equal(30, person1.Age);
        Assert.Equal("123 Main St", person1.Street);
        Assert.Equal("Springfield", person1.City);
        Assert.Equal("a root id", person1.RootId);
        Assert.Equal("root data", person1.RootData);
        var person2 = results[1].Value as TestPerson;
        Assert.Equal(2, person2.Id);
        Assert.Equal("Coucou", person2.FirstName);
        Assert.Null(person2.LastName);
        Assert.Equal(32, person2.Age);
        Assert.Null(person2.Street);
        Assert.Null(person2.City);
        Assert.Equal("a root id", person2.RootId);
        Assert.Equal("root data", person2.RootData);

        var companies = results.FindAll(i => i.NodeDefinitionName == "company").ToList();
        var company1 = companies[0].Value as TestCompany;
        Assert.Equal(3, company1.Id);
        Assert.Equal("MyCompany", company1.Name);
        Assert.Null(company1.Street);
        Assert.Equal("Big City", company1.City);
        Assert.Equal("a root id", company1.RootId);
        Assert.Equal("root data", company1.RootData);
    }
    [Fact]
    public void Read_SimpleXmlWithAttributes_ParsesCorrectly1()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                       <root rootId=""a root id"">
                         <data>root data</data>
                         <person id=""1"" firstName=""John"" lastName=""Doe"">
                           <age>30</age>
                         </person>
                       </root>";

        var definition = new XmlFileDefinition();
        definition.AddNodeDefinition(
            "person",
            "/root/person",
            i => new TestPerson
            {
                Id = i.ToXPathQuery<int>("/root/person/@id"),
                FirstName = i.ToXPathQuery<string>("/root/person/@firstName"),
                LastName = i.ToXPathQuery<string>("/root/person/@lastName"),
                Age = i.ToXPathQuery<int>("/root/person/age"),
                RootId = i.ToXPathQuery<string>("/root/@rootId"),
                RootData = i.ToXPathQuery<string>("/root/data"),
            });

        var results = new List<XmlNodeParsed>();
        var reader = new XmlObjectReaderV2(definition, "test", results.Add);

        // Act
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
        {
            reader.Read(stream, CancellationToken.None);
        }

        // Assert
        Assert.Single(results);
        var person = results[0].Value as TestPerson;
        Assert.Equal(1, person.Id);
        Assert.Equal("John", person.FirstName);
        Assert.Equal("Doe", person.LastName);
        Assert.Equal(30, person.Age);
        Assert.Equal("a root id", person.RootId);
        Assert.Equal("root data", person.RootData);
    }

    private class TestPerson
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string RootId { get; set; }
        public string RootData { get; set; }
    }
    private class TestCompany
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string RootId { get; set; }
        public string RootData { get; set; }
    }
}
