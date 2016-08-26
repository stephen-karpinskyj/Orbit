using UnityEngine;

public static class PaddlePhysics
{
    private const float NearZero = 0.001f;

    public static void ResolveTarget(int playerId, GameContext context, TransformTarget target)
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


            CheckDiscCollision(state, target, context);
        }
    }

    private static void WallSlideToTarget(int playerId, GameContext context, TransformTarget target)
    {
        var state = context.State.GetPaddle(playerId);
        var trans = state.Transform;

        // Stick to line
        var nextPos = MathUtility.NearestPointOnFiniteLine(state.SlideWall.Start, state.SlideWall.End, trans.Position);
        target.Distance -= Vector2.Distance(trans.Position, nextPos);
        trans.Position = nextPos;

        var slideHeading = state.SlideWall.Normal.Rotate(state.Direction == Direction.CW ? 90 : -90).normalized;
        var slideTargetPos = trans.Position + slideHeading * target.Distance;

        nextPos = MathUtility.NearestPointOnFiniteLine(state.SlideWall.Start, state.SlideWall.End, slideTargetPos);
        target.Distance -= Vector2.Distance(trans.Position, nextPos);
        trans.Position = nextPos;

        if (!BoxPhysics.IsInside(context.Config.PaddleBox, slideTargetPos))
        {
            if (state.SlideWall.Equals(state.TargetSlideWall))
            {
                state.Direction = state.Direction.Opposite();
            }
            else
            {
                state.SlideWall = context.Config.PaddleBox.GetNextWall(state.SlideWall, state.Direction);
            }
        }

        trans.Rotation = CalculateWallSlideRotation(state, context.State.Disc);
    }

    private static void MoveToTarget(int playerId, GameContext context, TransformTarget target)
    {
        var state = context.State.GetPaddle(playerId);
        var trans = state.Transform;

        var nextPos = trans.Position + target.Heading * target.Distance;
        var nextRot = trans.Rotation + target.AngleOffset;

        Line collisionWall;
        Vector2 collisionPoint;

        var collided = BoxPhysics.CheckPointCollision(context.Config.PaddleBox, trans.Position, nextPos, out collisionWall, out collisionPoint);
        if (collided)
        {
            // Calculate partial position offset
            var collisionPosAmount = Mathf.Max(0f, Vector2.Distance(trans.Position, collisionPoint) - NearZero);
            var collisionPosTarget = trans.Position + target.Heading * collisionPosAmount;

            // Apply partial position offset
            target.Distance -= collisionPosAmount;
            trans.Position = collisionPosTarget;

            // TODO: Assumes bottom line is player's goal line
            /*if (state.IsTapping && context.State.Disc.Transform.Position.y < -context.Config.Size.ZoneOffset)
            {
                state.Mode = PaddleState.MovementMode.WallSliding;
                state.Direction = CalculateWallSlideDirection(context.Config.PaddleBox, collisionWall, collisionPoint, context.State.Disc.Transform.Position);
                state.SlideWall = collisionWall;
                state.TargetSlideWall = context.Config.PaddleBox.GetWall(Box.WallType.Bottom);
                trans.Rotation = CalculateWallSlideRotation(state, context.State.Disc);
                target.AngleOffset = 0;
            }
            else*/
            {
                // Calculate partial rotation offset
                var bounceDistPercentage = collisionPosAmount / target.Distance;
                var bounceRotAmount = bounceDistPercentage * target.AngleOffset;

                // Reflect rotation against wall + apply partial rotation offset
                target.Heading = Vector2.Reflect(nextPos - trans.Position, collisionWall.Normal).Rotate(bounceRotAmount).normalized;
                target.AngleOffset -= bounceRotAmount;
                trans.Rotation = Vector2.up.SignedAngle(target.Heading);
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

    private static float CalculateWallSlideRotation(PaddleState state, DiscState discState)
    {
        var heading = (discState.Transform.Position - state.Transform.Position).normalized;
        return Vector2.up.SignedAngle(heading);
    }

    private static Direction CalculateWallSlideDirection(Box paddleBox, Line collisionWall, Vector2 collisionPoint, Vector2 discPosition)
    {
        if (collisionWall.Equals(paddleBox.GetWall(Box.WallType.Left)))
        {
            return Direction.CCW;
        }

        if (collisionWall.Equals(paddleBox.GetWall(Box.WallType.Right)))
        {
            return Direction.CW;
        }

        // TODO: Account for which goal the team paddle's is, currently assumes the goal is on bottom line 
        if (collisionWall.Equals(paddleBox.GetWall(Box.WallType.Top)))
        {
            return collisionPoint.x < 0f ? Direction.CCW : Direction.CW;
        }

        return discPosition.x > collisionPoint.x ? Direction.CCW : Direction.CW;
    }

    // TODO: Move to disc physics
    /// <remarks>Source: http://gamedevelopment.tutsplus.com/tutorials/when-worlds-collide-simulating-circle-circle-collisions--gamedev-769</remarks>
    private static void CheckDiscCollision(PaddleState state, TransformTarget target, GameContext context)
    {
        // Circle-circle collision
        var discRadius = context.Config.Disc.Size.Radius;
        var paddleRadius = context.Config.Paddle.Size.Radius;
        var discPos = context.State.Disc.Transform.Position;
        var paddlePos = state.Transform.Position;

        var discVelocity = context.State.Disc.Heading * context.State.Disc.Speed;
        var paddleVelocity = target.Heading * context.Config.Paddle.Size.ForwardSpeed;
        var discMass = context.Config.Disc.Size.Mass;
        var paddleMass = context.Config.Paddle.Size.Mass;

        var dist = Vector2.Distance(discPos, paddlePos);
        var minDist = discRadius + paddleRadius;

        if (dist < minDist)
        {
            var collisionPoint = Vector2.zero;
            collisionPoint.x = ((discPos.x * paddleRadius) + (paddlePos.x * discRadius)) / (discRadius + paddleRadius);
            collisionPoint.y = ((discPos.y * paddleRadius) + (paddlePos.y * discRadius)) / (discRadius + paddleRadius);

            var discNormal = (collisionPoint - discPos).normalized;
            var bounceHeading = -Vector2.Reflect(paddleVelocity, discNormal).normalized;

            var newDiscVelocity = Vector2.zero;
            newDiscVelocity.x = (discVelocity.x * (discMass - paddleMass) + (2 * paddleMass * paddleVelocity.x)) / (discMass + paddleMass);
            newDiscVelocity.y = (discVelocity.y * (discMass - paddleMass) + (2 * paddleMass * paddleVelocity.y)) / (discMass + paddleMass);

            context.State.Disc.Heading = (newDiscVelocity + bounceHeading).normalized;
            context.State.Disc.Speed = Mathf.Max(context.State.Disc.Speed, newDiscVelocity.magnitude * context.Config.Paddle.DiscBounceFactor);
        }
    }
}
