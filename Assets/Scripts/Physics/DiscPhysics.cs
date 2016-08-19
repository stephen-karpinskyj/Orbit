using UnityEngine;

public static class DiscPhysics
{
    private const float NearZero = 0.001f;

    public static void ResolveTarget(GameContext context, TransformTarget target)
    {
        while (target.Distance > NearZero)
        {
            MoveToTarget(context, target);
        }
    }

    private static void MoveToTarget(GameContext context, TransformTarget target)
    {
        var state = context.State.Disc;
        var trans = state.Transform;

        var nextPos = trans.Position + target.Heading * target.Distance;

        Line collisionWall;
        Vector2 collisionPoint;

        var collided = TablePhysics.CheckPointCollision(context.Config.DiscBox, trans.Position, nextPos, out collisionWall, out collisionPoint);
        if (collided)
        {
            // Calculate partial position offset
            var collisionPosAmount = Mathf.Max(0f, Vector2.Distance(trans.Position, collisionPoint) - NearZero);
            var collisionPosTarget = trans.Position + target.Heading * collisionPosAmount;

            // Apply partial position offset
            target.Distance -= collisionPosAmount;
            trans.Position = collisionPosTarget;

            // Reflect heading against wall
            target.Heading = Vector2.Reflect(target.Heading, collisionWall.Normal).normalized;
            state.Heading = target.Heading;
        }
        else
        {
            target.Distance = 0f;
            target.AngleOffset = 0f;
            trans.Position = nextPos;
        }
    }
}
