using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using AWiring.Models;

namespace AWiring.ViewModels;

internal partial class Scheme : ViewModelBase {
    public Dictionary<string, Element> Elements { get; set; } = new();
    [ObservableProperty] private string name;
    [ObservableProperty] private bool allowDiagonalWires;

    public ICollection<Potential> Potentials { get; } = new List<Potential>();

    public Scheme(string name) {
        this.name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public Wire Wire(Pole src, Pole dst) {
        if (src.Element.Scheme != this)
            throw new ArgumentException("source elemenet does not belong to the current scheme", nameof(src));
        if (dst.Element.Scheme != this)
            throw new ArgumentException("destination elemenet does not belong to the current scheme", nameof(dst));

        if (src.Wires.TryGetValue(dst, out var wire))
            return wire;    // nothing to do

        if (src.Potential != dst.Potential) {
            // combine potentials
            while (dst.Potential.Poles.Count > 0) {
                // move poles one-by-one intentionally
                var pole = dst.Potential.Poles.First();
                dst.Potential.Poles.Remove(pole);
                src.Potential.Poles.Add(pole);
            }
            dst.Potential = src.Potential;
        }

        wire = new Wire(src.Element.Scheme, src, dst);
        src.Wires.Add(dst, wire);
        dst.Wires.Add(src, wire);
        return wire;
    }

    public void Unwire(Pole src, Pole dst) {
        if (src.Element.Scheme != this)
            throw new ArgumentException("source elemenet does not belong to the current scheme", nameof(src));
        if (dst.Element.Scheme != this)
            throw new ArgumentException("destination elemenet does not belong to the current scheme", nameof(dst));

        UndirectedGraph<Pole> graph = new(src.Potential.Wires.Select(w => (w.Src, w.Dst)));
        graph.RemoveEdge(src, dst);
        if (!graph.IsReachable(src, dst)) {
            var graphs = graph.SplitIntoConnectedGraphs();  // there must be exactly two
            src.Potential = new Potential(graphs.Where(g => g.Verticies.Contains(src)).First().Verticies);
            dst.Potential = new Potential(graphs.Where(g => g.Verticies.Contains(dst)).First().Verticies);
        }
        src.Wires.Remove(dst);
        dst.Wires.Remove(src);
    }
}

internal partial class CableModel : ViewModelBase {
    public int Id { get; set; }

    /// <summary>
    /// Марка кабеля.
    /// </summary>
    /// <example>ВВГнг(А)-LS</example>
    public string Name { get; set; }
    public string Description { get; set; } = "";
    public IList<WireModel> Wires { get; }

    /// <summary>
    /// Наружный диаметр кабеля.
    /// </summary>
    public float Diameter1 { get; set; }

    /// <summary>
    /// Совпадает с <see cref="Diameter1"/> для круглых кабелей.
    /// </summary>
    public float Diameter2 { get; set; }

    /// <summary>
    /// Минимальная толщина изгиба, в диаметрах кабеля.
    /// </summary>
    public float MinRound { get; set; } = 10;

    /// <summary>
    /// Указывается ли для кабеля традиционно поперечное сечение жил (true), или же их диаметр (false).
    /// </summary>
    public bool UseAreaForWires { get; set; } = true;

    public CableModel(int id, string name, params WireModel[] wires) {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        if (wires.Length <= 0)
            throw new ArgumentOutOfRangeException(nameof(wires));
        Wires = wires ?? throw new ArgumentNullException(nameof(wires));
    }
}

internal record WireColor(Color Primary, Color? Secondary = null, float Relation = 0.5f) {
    public static readonly WireColor Black = new(Colors.Black);
}

internal partial class WireModel : ViewModelBase {
    [ObservableProperty] private WireColor color = WireColor.Black;
    [ObservableProperty] private float diameter;
}

internal partial class Wire : ViewModelBase {
    public Scheme Scheme { get; }
    public Pole Src { get; }
    public Pole Dst { get; }

    [ObservableProperty] private string circuit = "";
    [ObservableProperty] private Label? label;
    [ObservableProperty] private float width = 1;
    [ObservableProperty] private float rounding = 0;

    private readonly ObservableCollection<WireSegment> segments = new();
    public ReadOnlyObservableCollection<WireSegment> Segments { get; }

    /// <summary>
    /// Find other wires sharing same potential
    /// </summary>
    public IEnumerable<Wire> ComposedWith() => Src.Potential.Wires.Where(w => w != this);

    /// <summary>
    /// 
    /// </summary>
    public static readonly float CircularWireOffset = 10;

    public Wire(Scheme scheme, Pole src, Pole dst) {
        if (src == dst)
            throw new ArgumentException("wire should not start and end at the same pole", nameof(dst));

        Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
        Segments = new(segments);
        Src = src ?? throw new ArgumentNullException(nameof(src));
        Dst = dst ?? throw new ArgumentNullException(nameof(dst));

        Rebuild();
    }

    public void Rebuild() {
        segments.Clear();
        APoint[] midPoints;
        var prev = Src.Center;

        if (Src.Center.X == Dst.Center.X && Src.Center.Y == Dst.Center.Y) {
            midPoints = new APoint[]
            {
                Src.Center.Offset(CircularWireOffset, 0),
                Src.Center.Offset(CircularWireOffset, CircularWireOffset),
                Src.Center.Offset(0, CircularWireOffset),
            };
        } else if (Scheme.AllowDiagonalWires || Src.Center.X == Dst.Center.X || Src.Center.Y == Dst.Center.Y) {
            midPoints = Array.Empty<APoint>();
        } else {
            var xmiddle = (Dst.Center.X - Src.Center.X) / 2;
            midPoints = new APoint[]
            {
                Src.Center.Offset(xmiddle, 0),
                Dst.Center.Offset(-xmiddle, 0)
            };
        }

        foreach (var pt in midPoints) {
            segments.Add(new(prev, pt));
            prev = pt;
        }
        segments.Add(new(prev, Dst.Center));
    }

    partial void OnRoundingChanging(float value) {
        throw new NotImplementedException();
    }
}

internal partial class WireSegment : ViewModelBase {
    public WireSegment(APoint start, APoint end) {
        Start = start;
        End = end;
    }

    [ObservableProperty] private APoint start;
    [ObservableProperty] private APoint end;

    public (WireSegment, WireSegment) Split(APoint where) {
        return (new(Start, where), new(where, End));
    }
}

internal partial class Potential : ViewModelBase {
    public ISet<Pole> Poles { get; }
    public IEnumerable<Wire> Wires => Poles.SelectMany(pole => pole.Wires.Values).Distinct();

    public Potential() {
        Poles = new HashSet<Pole>();
    }

    public Potential(IEnumerable<Pole> poles) {
        Poles = new HashSet<Pole>(poles);
    }
}

internal partial class Pole : ViewModelBase {
    [ObservableProperty] private string name = "";
    [ObservableProperty] private APoint center;
    [ObservableProperty] private float length = 0.2f;
    [ObservableProperty] private Orientation orientation;

    public APoint ConnectPoint => center;
    public Element Element { get; }
    public Potential Potential { get; set; } = new();

    public Pole(Element element) {
        Element = element ?? throw new ArgumentNullException(nameof(element));
    }

    private readonly Dictionary<Pole, Wire> wires = new();
    public IDictionary<Pole, Wire> Wires => wires;

    partial void OnNameChanging(string value) {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
    }
}

internal class Element : ViewModelBase {
    public string Id { get => this["Id"]; set => this["Id"] = value; }
    public Scheme Scheme { get; }

    public readonly IList<Pole> poles = new List<Pole>();

    public Dictionary<string, string> Attributes { get; } = new();

    public string this[string propName] {
        get { return Attributes[propName]; }
        set {
            if (propName == "Id" && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Element ID cannot be blank");
            if (string.IsNullOrEmpty(value))
                Attributes.Remove(propName);
            else
                Attributes[propName] = value;
        }
    }

    public Element(Scheme scheme, string id) {
        Id = id;
        Scheme = scheme;
    }
}

internal partial class Label : ViewModelBase {
    [ObservableProperty] private string text = "";
    [ObservableProperty] private FontFamily fontFamily = new("serif");
    [ObservableProperty] private float fontSize = 6;   // pt
    [ObservableProperty] private float orientation;
}
