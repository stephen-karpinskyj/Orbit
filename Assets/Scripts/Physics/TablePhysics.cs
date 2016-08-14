using UnityEngine;

public static class TablePhysics
{
    public static bool CheckPointCollision(TableConfig config, Vector2 currPoint, Vector2 targetPoint, out Line collisionWall, out Vector2 collisionPoint)
    {
        collisionWall = default(Line);
        collisionPoint = default(Vector2);

        if (config.Bounds.Contains(targetPoint))
        {
            return false;
        }

        // FIXME: Need better way to choose which wall collision was with, can be incorrect if an object goes out of bounds in both x and y (ie. corner) in the same frame
        // TODO: Solve this more generally if we change wall layout
        if (targetPoint.x < config.Bounds.min.x)
        {
            collisionWall = config.GetWall(TableConfig.WallType.Left);
        }
        else if (targetPoint.x > config.Bounds.max.x)
        {
            collisionWall = config.GetWall(TableConfig.WallType.Right);
        }
        else if (targetPoint.y < config.Bounds.min.y)
        {
            collisionWall = config.GetWall(TableConfig.WallType.Bottom);
        }
        else if (targetPoint.y > config.Bounds.max.y)
        {
            collisionWall = config.GetWall(TableConfig.WallType.Top);
        }
        else
        {
            throw new System.Exception();
        }

        MathUtility.LineSegmentIntersection(currPoint, targetPoint, collisionWall.Start, collisionWall.End, out collisionPoint);
        return true;
    }

    public static bool IsInside(TableConfig config, Vector2 point)
    {
        return config.Bounds.SqrDistance(point) <= Mathf.Epsilon;
    }
}
