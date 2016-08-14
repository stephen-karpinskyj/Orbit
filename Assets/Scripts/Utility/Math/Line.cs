using UnityEngine;

public struct Line
{
    public Vector2 Start;
    public Vector2 End;
    public Vector2 Mid;
    public Vector2 Normal;

    public Line(Vector2 start, Vector2 end)
    {
        this.Start = start;
        this.End = end;

        var v = (end - start).normalized;
        this.Mid = start + v * ((end - start).magnitude / 2);
        this.Normal = new Vector2(-v.y, v.x);
    }
}