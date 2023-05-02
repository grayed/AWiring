namespace AWiring.Models;

public readonly struct AVector {
    public readonly Orientation Angle;
    public readonly float Length;
    private readonly float X, Y;

    public AVector Rotate(float angle) {
        return new(new Orientation(Angle + angle), Length);
    }

    public AVector(Orientation angle, float length) {
        Angle = angle;
        Length = length;
        X = (float)(Math.Cos(Angle) * Length);
        Y = (float)(Math.Sin(Angle) * Length);
    }

    public AVector(APoint pt) {
        X = pt.X;
        Y = pt.Y;
        Angle = (float)Math.Atan2(X, Y);
        Length = (float)Math.Sqrt(X * X + Y * Y);
    }

    public static explicit operator APoint(AVector v) => new((float)(Math.Cos(v.Angle) * v.Length), (float)(Math.Sin(v.Angle) * v.Length));
    public static explicit operator AVector(APoint pt) => new(pt);

    public static AVector operator +(AVector v1, AVector v2) => new(new APoint(v1.X + v2.X, v1.Y + v2.Y));
    public static AVector operator -(AVector v1, AVector v2) => new(new APoint(v1.X - v2.X, v1.Y - v2.Y));
    public static AVector operator *(AVector v, float coef) => new(v.Angle, v.Length * coef);
}
