namespace AWiring.Models;

public readonly struct Orientation {
    /// <summary>
    /// Always in the [0; 2*pi) range.
    /// </summary>
    public readonly float Angle;

    public Orientation(float value) {
        if (!float.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "must be a finite value");
        Angle = Normalize(value);
    }

    private const float PiQ = float.Pi / 4;

    /// <summary>
    /// Convert arbitrary orientation to a pure horizontal or vertical one.
    /// </summary>
    /// <returns>0, Pi/2, Pi or 3Pi/2</returns>
    public Orientation NonDiagonal() {
        if (Angle < 1 * PiQ || Angle >= 7 * PiQ)
            return 0;
        else if (Angle >= 1 * PiQ && Angle < 3 * PiQ)
            return 2 * PiQ;
        else if (Angle >= 3 * PiQ && Angle < 5 * PiQ)
            return 4 * PiQ;
        else // if (Value >= 5 * PiQ && Value < 7 * PiQ)
            return 6 * PiQ;
    }

    private static float Normalize(float value) {
        value %= (float)(Math.PI * 2);
        if (value < 0)
            value += (float)(Math.PI * 2);
        return value;
    }

    public bool IsRight => Angle < 2 * PiQ || Angle > 6 * PiQ;
    public bool IsLeft => Angle > 2 * PiQ && Angle < 6 * PiQ;
    public bool IsUp => Angle > 0 && Angle < Math.PI;
    public bool IsDown => Angle > Math.PI;  // normalization won't allow the Pi*2 value for angle

    public bool IsHorizontal => Angle < 1 * PiQ || Angle >= 7 * PiQ || (Angle >= 3 * PiQ && Angle < 5 * PiQ);
    public bool IsVertical => (Angle >= 1 * PiQ && Angle < 3 * PiQ) || (Angle >= 5 * PiQ && Angle < 7 * PiQ);

    public static implicit operator float(Orientation o) => o.Angle;
    public static implicit operator Orientation(float value) => new(value);
}
