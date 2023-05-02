namespace AWiring;

public class UndirectedGraph<T> : IEquatable<UndirectedGraph<T>?> where T : notnull {
    private readonly int[,] edges;
    private readonly Dictionary<T, int> verticies;
    private bool isDirty = true;

    public IEnumerable<T> Verticies => verticies.Keys;

    public UndirectedGraph(IEnumerable<T> verticies) {
        this.verticies = new Dictionary<T, int>(verticies.Select((v, i) => new KeyValuePair<T, int>(v, i)));
        if (this.verticies.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(verticies), "missing input data");
        edges = Init();
    }

    public UndirectedGraph(IEnumerable<(T, T)> pairs) {
        this.verticies = new Dictionary<T, int>();
        var idxPairs = new List<(int, int)>();
        foreach (var pair in pairs) {
            if (!verticies.TryGetValue(pair.Item1, out var idx1)) {
                idx1 = verticies.Count;
                verticies.Add(pair.Item1, idx1);
            }
            if (!verticies.TryGetValue(pair.Item2, out var idx2)) {
                idx2 = verticies.Count;
                verticies.Add(pair.Item2, idx2);
            }
            idxPairs.Add((idx1, idx2));
        }
        if (this.verticies.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(pairs), "missing input data");

        edges = Init();
        foreach (var idxPair in idxPairs) {
            edges[idxPair.Item1, idxPair.Item2] = 1;
            edges[idxPair.Item2, idxPair.Item1] = 1;
        }
    }

    private int[,] Init() {
        var edges = new int[verticies.Count, verticies.Count];
        for (int i = 0; i < verticies.Count; i++) {
            for (int j = 0; j < verticies.Count; j++)
                edges[i, j] = int.MaxValue;
            edges[i, i] = 0;
        }
        return edges;
    }

    public void AddEdge(T from, T to) {
        int srcIdx = verticies[from];
        int dstIdx = verticies[to];
        edges[srcIdx, dstIdx] = 1;
        edges[dstIdx, srcIdx] = 1;
        isDirty = true;
    }

    public void RemoveEdge(T from, T to) {
        int srcIdx = verticies[from];
        int dstIdx = verticies[to];
        edges[srcIdx, dstIdx] = int.MaxValue;
        edges[dstIdx, srcIdx] = int.MaxValue;
        isDirty = true;
    }

    public bool IsReachable(T from, T to) {
        CompletePaths();
        return edges[verticies[from], verticies[to]] != int.MaxValue;
    }

    public bool IsConnected {
        get {
            CompletePaths();
            for (int i = 1; i < verticies.Count; i++)
                if (edges[0, i] == int.MaxValue)
                    return false;
            return true;
        }
    }

    /// <summary>
    /// Выделяет области связности текущего графа в виде отдельных графов .
    /// </summary>
    /// <returns>
    /// Набор не пересекающихся связных графов, каждый из которых состоит из вершин и изначально заданных рёбер текущего графа.
    /// </returns>
    public IEnumerable<UndirectedGraph<T>> SplitIntoConnectedGraphs() {
        CompletePaths();
        var processedIndexes = new HashSet<int>(verticies.Count);
        var vlist = verticies.Select(kv => kv.Key).ToList();
        for (int i = 0; i < verticies.Count; i++) {
            if (processedIndexes.Contains(i))
                continue;
            processedIndexes.Add(i);
            var subgraphIndexes = new List<int> { i };

            var pairs = new List<(T, T)>();
            for (int j = verticies.Count - 1; j >= 0; j--)
                if (!processedIndexes.Contains(j) && edges[i, j] != int.MaxValue) {
                    subgraphIndexes.Add(j);
                    processedIndexes.Add(j);
                }

            // carefully pass only edges that were specified manually
            for (int j = 0; j < subgraphIndexes.Count; j++)
                for (int k = 0; k < subgraphIndexes.Count; k++)
                    if (edges[subgraphIndexes[j], subgraphIndexes[k]] == 1)
                        pairs.Add((vlist[subgraphIndexes[j]], vlist[subgraphIndexes[k]]));

            if (pairs.Count > 0)
                yield return new UndirectedGraph<T>(pairs);
            else
                yield return new UndirectedGraph<T>(new T[] { vlist[i] });
        }
    }

    /// <summary>
    /// Отмечает прямые пути между вершинами, между которыми уже есть какой-либо путь.
    /// </summary>
    private void CompletePaths() {
        if (!isDirty)
            return;
        ResetComputedPaths();
        for (int k = 0; k < verticies.Count; k++)
            for (int i = 0; i < verticies.Count; i++)
                for (int j = 0; j < verticies.Count; j++) {
                    if (edges[i, j] == int.MaxValue && edges[k, i] != int.MaxValue && edges[k, j] != int.MaxValue) {
                        edges[i, j] = 2;
                        edges[j, i] = 2;
                    }
                }
    }

    private void ResetComputedPaths() {
        for (int i = 0; i < verticies.Count; i++)
            for (int j = 0; j < verticies.Count; j++)
                if (edges[i, j] > 1)
                    edges[i, j] = int.MaxValue;
    }

    public override bool Equals(object? obj) {
        return Equals(obj as UndirectedGraph<T>);
    }

    public bool Equals(UndirectedGraph<T>? other) {
        return other is not null &&
               EqualityComparer<int[,]>.Default.Equals(edges, other.edges) &&
               EqualityComparer<Dictionary<T, int>>.Default.Equals(verticies, other.verticies) &&
               isDirty == other.isDirty;
    }

    public override int GetHashCode() {
        return HashCode.Combine(edges, verticies, isDirty);
    }

    public static bool operator ==(UndirectedGraph<T>? left, UndirectedGraph<T>? right) {
        return EqualityComparer<UndirectedGraph<T>>.Default.Equals(left, right);
    }

    public static bool operator !=(UndirectedGraph<T>? left, UndirectedGraph<T>? right) {
        return !(left == right);
    }
}
