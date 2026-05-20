using Xunit;

// Tests that perform memory-sensitive measurements must not run in parallel
// with tests that allocate large amounts of working memory (like the heavy
// finance pipeline). Put both in the same xUnit collection so they execute
// sequentially with respect to each other.
[CollectionDefinition("MemorySensitive")]
public class MemorySensitiveCollection { }
