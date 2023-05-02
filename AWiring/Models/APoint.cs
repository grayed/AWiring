namespace AWiring.Models;

public readonly struct APoint {
    public readonly float X;
    public readonly float Y;

    public APoint(float x, float y) {
        X = x;
        Y = y;
    }

    public APoint Offset(float xOff, float yOff) => new(X + xOff, Y + yOff);
    public APoint Offset(AVector v) => (APoint)(new AVector(this) + v);
}
