using UnityEngine;

public struct HexPos
{
    public int X;
    public int Y;

    public HexPos(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(HexPos obj)
    {
        return obj.X == X && obj.Y == Y;
    }

    public override string ToString()
    {
        return "X" + X + "Y" + Y;
    }

    public Directions DirectionTo(HexPos hexPos)
    {
        if (hexPos.X == X && hexPos.Y == Y) return Directions.None;
        if (hexPos.X == X && hexPos.Y != Y)
        {
            return hexPos.Y > Y ? Directions.OY : Directions.OY_Rev;
        }

        if (hexPos.Y == Y && hexPos.X != X)
        {
            return hexPos.X > X ? Directions.OX : Directions.OX_Rev;
        }

        if (hexPos.X + hexPos.Y == X + Y)
        {
            return hexPos.X > X ? Directions.OZ_Rev : Directions.OZ;
        }

        return Directions.None;
    }

    public bool IsAdjacentTo(HexPos hexPos)
    {
        int deltaX = X - hexPos.X;
        int deltaY = Y - hexPos.Y;

        if (deltaX == 0 && Mathf.Abs(deltaY) == 1) return true;
        if (deltaY == 0 && Mathf.Abs(deltaX) == 1) return true;
        if (deltaX + deltaY == 0 && Mathf.Abs(deltaX) == 1) return true;

        return false;
    }

    public static HexPos operator +(HexPos hp1, HexPos hp2)
    {
        return new HexPos(hp1.X + hp2.X, hp1.Y + hp2.Y);
    }

    public static HexPos operator -(HexPos hp1, HexPos hp2)
    {
        return new HexPos(hp1.X - hp2.X, hp1.Y - hp2.Y);
    }

    public static bool operator ==(HexPos hp1, HexPos hp2)
    {
        return hp1.X == hp2.X && hp1.Y == hp2.Y;
    }

    public static bool operator !=(HexPos hp1, HexPos hp2)
    {
        return hp1.X != hp2.X || hp1.Y != hp2.Y;
    }

    public static HexPos operator *(HexPos hp1, int factor)
    {
        return new HexPos(hp1.X * factor, hp1.Y * factor);
    }

    public static HexPos operator *(int factor, HexPos hp1)
    {
        return new HexPos(hp1.X * factor, hp1.Y * factor);
    }

    public static HexPos Hex_OX = new HexPos(1, 0);
    public static HexPos Hex_OY = new HexPos(0, 1);
    public static HexPos Hex_OZ = new HexPos(-1, 1);

    public static HexPos Empty = new HexPos(-1000, -1000);

    public enum Directions
    {
        None,
        OX,
        OY,
        OZ,
        OX_Rev,
        OY_Rev,
        OZ_Rev,
    }
}