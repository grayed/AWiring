namespace AWiring.Tests;

public class GraphTests {
    readonly (string, string)[] splitPairs = new (string, string)[]
    {
            ("v1", "v2"),
            ("v2", "v3"),
            ("v3", "v4"),
            ("v1", "v2"),
            ("v5", "v6"),
    };

    readonly (string, string)[] connectedPairs = new (string, string)[]
    {
                ("v1", "v2"),
                ("v2", "v3"),
                ("v3", "v4"),
                ("v2", "v4"),
                ("v1", "v5"),
                ("v5", "v6"),
    };

    [SetUp]
    public void Setup() {
    }

    [TestCase("v1", "v2", ExpectedResult = false)]
    [TestCase("v1", "v5", ExpectedResult = true)]
    [TestCase("v3", "v5", ExpectedResult = true)]
    [TestCase("v4", "v6", ExpectedResult = true)]
    public bool TestConnectivityAfterAdding(string from, string to) {
        var graph = new UndirectedGraph<string>(splitPairs);
        graph.AddEdge(from, to);
        return graph.IsConnected;
    }

    [TestCase("v1", "v2", ExpectedResult = false)]
    [TestCase("v5", "v6", ExpectedResult = false)]
    [TestCase("v3", "v5", ExpectedResult = true)]
    [TestCase("v2", "v4", ExpectedResult = true)]
    public bool TestConnectivityAfterRemoval(string from, string to) {
        var graph = new UndirectedGraph<string>(connectedPairs);
        graph.RemoveEdge(from, to);
        return graph.IsConnected;
    }

    [Test]
    public void TestSplittingWithSingleResult() {
        var graph = new UndirectedGraph<string>(splitPairs);
        var subGraphs = graph.SplitIntoConnectedGraphs();
        Assert.AreEqual(subGraphs.First(), graph);
    }
}
