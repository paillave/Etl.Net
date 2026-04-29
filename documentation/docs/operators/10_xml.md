---
sidebar_position: 10
---

# XML files

`Paillave.Etl.XmlFile` parses XML documents declaratively: you describe
the nodes you care about with their XPaths, and the operator emits one
strongly-typed instance per match — without loading the whole document
in memory (the parser is forward-only, SAX-style).

| Operator | Purpose |
| --- | --- |
| `CrossApplyXmlFile` | Parse an `IFileValue`, produce a stream of `XmlNodeParsed` |
| `XmlNodeOfType<T>` | Filter that stream and emit typed `T` instances for one node definition |
| `XmlNodeOfTypeCorrelated<T>` | Same, but also tag each row with the parent's correlation key |

> All snippets below are exercised by
> `src/Paillave.Etl.Tests/DocExamples/XmlOperatorsExamplesTests.cs`.

## Defining the document

The `XmlFileDefinition` lists every node you want to extract.
Each `AddNodeDefinition<T>(name, xPath, mapper)` call binds:
- a **node name** (used later by `XmlNodeOfType<T>` to filter),
- the **XPath** of the node in the document,
- a mapping lambda that uses an `IXmlFieldMapper`.

| `IXmlFieldMapper` method | Purpose |
| --- | --- |
| `ToXPathQuery<T>("path")` | Read a value from an absolute XPath |
| `ToXPathQuery<T>("path", depthScope)` | Bound the look-up to a relative depth |
| `ToSourceName()` | File name of the parsed document |
| `ToRowGuid()` | Unique row identifier |

> **XPaths are absolute.** Always start with `/root/...` rather than a
> relative `@id`. The mapper resolves XPaths against the **document
> root**, not the current node.

## Parsing one node type

> Test: `XmlNodeOfType_ParsesAttributesAndChildren`

```cs {3-12}
root
    .CrossApply("source", _ => new[] { fileValue })
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
    .Do("collect", p => Console.WriteLine(p.FirstName));
```

`@attribute` syntax reads attributes; the bare path reads the element's
inner text. Numeric / boolean conversions are performed automatically
based on `T`.

## Parsing several node types

> Test: `XmlNodeOfType_ParsesHeterogeneousNodes`

```cs {4-20}
var parsed = root
    .CrossApply("source", _ => new[] { fileValue })
    .CrossApplyXmlFile("parse", d => d
        .AddNodeDefinition<Person>("person", "/root/person",
            m => new Person
            {
                Id        = m.ToXPathQuery<int>("/root/person/@id"),
                FirstName = m.ToXPathQuery<string>("/root/person/@firstName"),
                Age       = m.ToXPathQuery<int>("/root/person/age"),
            })
        .AddNodeDefinition<Company>("company", "/root/company",
            m => new Company
            {
                Id   = m.ToXPathQuery<int>("/root/company/@id"),
                Name = m.ToXPathQuery<string>("/root/company/@name"),
            }));

parsed.XmlNodeOfType<Person>("p",  "person").Do("p+", p => /*...*/);
parsed.XmlNodeOfType<Company>("c", "company").Do("c+", c => /*...*/);
```

A single pass over the file produces both `Person` and `Company`
instances. The shared `parsed` stream can be split with
[`XmlNodeOfType<T>`](#) using the **node-definition name** as a
discriminator.

## Correlated parsing — children with their parent

> Test: `XmlNodeOfTypeCorrelated_GroupsChildrenUnderParent`

```cs {12}
.CrossApplyXmlFile("parse", d => d
    .AddNodeDefinition<Person>("person", "/root/person",
        m => new Person
        {
            Id        = m.ToXPathQuery<int>("/root/person/@id"),
            FirstName = m.ToXPathQuery<string>("/root/person/@firstName"),
        })
    .AddNodeDefinition<Pet>("pet", "/root/person/pet",
        m => new Pet { Name = m.ToXPathQuery<string>("/root/person/pet/@name") }))
.XmlNodeOfTypeCorrelated<Pet>("pets", "/root/person", "pet")
.Do("group", p =>
{
    // p.Row is the Pet instance
    // p.CorrelationKeys carries a token for the enclosing /root/person
    Console.WriteLine($"pet={p.Row.Name} keys={string.Join(',', p.CorrelationKeys)}");
});
```

`XmlNodeOfTypeCorrelated<T>(name, correlationPath, nodeDefinitionName)`
emits one `Correlated<T>` per child. Every child carries the same
correlation token as its enclosing parent at `correlationPath`, which
makes it trivial to:

- regroup children with their parent via
  [aggregation operators](./1_core.md#aggregations),
- attach lookups by parent identity,
- preserve hierarchy through later joins.

## Namespaces

```cs
.CrossApplyXmlFile("parse", d => d
    .AddNameSpace("ns", "http://example.com/ns")
    .AddNodeDefinition<Foo>("foo", "/ns:root/ns:foo",
        m => new Foo { Bar = m.ToXPathQuery<string>("/ns:root/ns:foo/@bar") }))
```

Use `AddNameSpace(prefix, uri)` (or `AddNameSpaces(dict)`) before
declaring the node definitions. Then prefix every element in your
XPaths.

## Cheat sheet

| Intent | Snippet |
| --- | --- |
| Parse one node type | `.CrossApplyXmlFile(n, d => d.AddNodeDefinition<T>("name", "/path", m => ...)).XmlNodeOfType<T>(n2, "name")` |
| Multiple node types | `.AddNodeDefinition<A>(...).AddNodeDefinition<B>(...)` then split with two `.XmlNodeOfType<...>` |
| Children + parent context | `.XmlNodeOfTypeCorrelated<TChild>(n, "/path/to/parent", "childName")` |
| Read attribute | `m.ToXPathQuery<string>("/root/person/@id")` |
| Read element text | `m.ToXPathQuery<int>("/root/person/age")` |
| File name as column | `m.ToSourceName()` |
| Stable row id | `m.ToRowGuid()` |
| XML namespaces | `d.AddNameSpace("ns", uri)` then prefix XPaths |
