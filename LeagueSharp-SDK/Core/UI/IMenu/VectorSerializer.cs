using SharpDX;
using System;

[Serializable()]
public struct SerializableVector2
{
    /// <summary>
    /// x component
    /// </summary>
    public float X;

    /// <summary>
    /// y component
    /// </summary>
    public float Y;

    public SerializableVector2(float x, float y)
    {
        this.X = x;
        this.Y = y;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }

    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", X, Y);
    }

    public static implicit operator Vector2(SerializableVector2 rValue)
    {
        return new Vector2(rValue.X, rValue.Y);
    }

    public static implicit operator SerializableVector2(Vector2 rValue)
    {
        return new SerializableVector2(rValue.X, rValue.Y);
    }
}