using UnityEngine;

public static class PaddlePhysics
{
    private const float NearZero = 0.001f;

    public static void ResolveTarget(int playerId, GameContext context, PaddleControllerTarget target)
    {
        var state = context.State.GetPaddle(playerId);

        while (target.Distance > NearZero)
        {
            if (state.Mode == PaddleState.MovementMode.WallSliding)
            {
                WallSlideToTarget(playerId, context, target);
            }
            else
            {
                MoveToTarget(playerId, context, target);
            }
        }
    }

    private static void WallSlideToTarget(int playerId, GameContext context, PaddleControllerTarget target)
    {
        var state = context.State.GetPaddle(playerId);
        var trans = state.Transform;

        // Stick to line
        var nextPos = MathUtility.NearestPointOnFiniteLine(state.SlideWall.Start, state.SlideWall.End, trans.Position);
        target.Distance -= Vector2.Distance(trans.Position, nextPos);
        trans.Position = nextPos;

        var slideDirection = state.SlideWall.Normal.Rotate(state.Direction == Direction.CW ? 90 : -90).normalized;
        var slideTargetPos = trans.Position + slideDirection * target.Distance;

        nextPos = MathUtility.NearestPointOnFiniteLine(state.SlideWall.Start, state.SlideWall.End, slideTargetPos);
        target.Distance -= Vector2.Distance(trans.Position, nextPos);
        trans.Position = nextPos;

        if (!TablePhysics.IsInside(context.Config.Table, slideTargetPos))
        {
            state.SlideWall = context.Config.Table.GetNextWall(state.SlideWall, state.Direction);
        }

        trans.Rotation = CalculateWallSlideRotation(state);
    }

    private static void MoveToTarget(int playerId, GameContext context, PaddleControllerTarget target)
    {
        var state = context.State.GetPaddle(playerId);
        var trans = state.Transform;

        var nextPos = trans.Position + target.Direction * target.Distance;
        var nextRot = trans.Rotation + target.AngleOffset;

        Line collisionWall;
        Vector2 collisionPoint;

        var collided = TablePhysics.CheckPointCollision(context.Config.Table, trans.Position, nextPos, out collisionWall, out collisionPoint);
        if (collided)
        {
            // Calculate partial position offset
            var collisionPosAmount = Mathf.Max(0f, Vector2.Distance(trans.Position, collisionPoint) - NearZero);
            var collisionPosTarget = trans.Position + target.Direction * collisionPosAmount;

            // Apply partial position offset
            target.Distance -= collisionPosAmount;
            trans.Position = collisionPosTarget;

            if (state.IsTapping)
            {
                state.Mode = PaddleState.MovementMode.WallSliding;
                state.SlideWall = collisionWall;
                trans.Rotation = CalculateWallSlideRotation(state);
                target.AngleOffset = 0;
            }
            else
            {
                // Calculate partial rotation offset
                var bounceDistPercentage = collisionPosAmount / target.Distance;
                var bounceRotAmount = bounceDistPercentage * target.AngleOffset;

                // Reflect rotation against wall + apply partial rotation offset
                target.Direction = Vector2.Reflect(nextPos - trans.Position, collisionWall.Normal).Rotate(bounceRotAmount).normalized;
                target.AngleOffset -= bounceRotAmount;
                trans.Rotation = Vector2.up.SignedAngle(target.Direction);
            }
        }
        else
        {
            target.Distance = 0f;
            target.AngleOffset = 0f;
            trans.Position = nextPos;
            trans.Rotation = nextRot;
        }
    }

    private static float CalculateWallSlideRotation(PaddleState state)
    {
        return Vector2.up.SignedAngle(state.SlideWall.Normal);
    }
}
