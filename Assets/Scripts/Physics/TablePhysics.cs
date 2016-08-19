using UnityEngine;

public static class TablePhysics
{
    public static bool CheckPointCollision(Box box, Vector2 currPoint, Vector2 targetPoint, out Line collisionWall, out Vector2 collisionPoint)
    {
        // Circle-line segment collision
        collisionWall = default(Line);
        collisionPoint = default(Vector2);

        if (box.Bounds.Contains(targetPoint))
        {
            return false;
        }

        // FIXME: Need better way to choose which wall collision was with, can be incorrect if an object goes out of bounds in both x and y (ie. corner) in the same frame
        // TODO: Solve this more generally if we change wall layout
        if (targetPoint.x < box.Bounds.min.x)
        {
            collisionWall = box.GetWall(Box.WallType.Left);
        }
        else if (targetPoint.x > box.Bounds.max.x)
        {
            collisionWall = box.GetWall(Box.WallType.Right);
        }
        else if (targetPoint.y < box.Bounds.min.y)
        {
            collisionWall = box.GetWall(Box.WallType.Bottom);
        }
        else if (targetPoint.y > box.Bounds.max.y)
        {
            collisionWall = box.GetWall(Box.WallType.Top);
        }
        else
        {
            throw new System.Exception();
        }

        MathUtility.LineSegmentIntersection(currPoint, targetPoint, collisionWall.Start, collisionWall.End, out collisionPoint);
        return true;
    }

    public static bool IsInside(Box config, Vector2 point)
    {
        return config.Bounds.SqrDistance(point) <= Mathf.Epsilon;
    }
}
