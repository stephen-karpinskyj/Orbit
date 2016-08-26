using UnityEngine;

public static class PaddlePhysics
{
    private const float NearZero = 0.001f;

    public static void ResolveTarget(int playerId, GameContext context, TransformTarget target)
    {
        var state = context.State.GetPaddle(playerId);

        while (target.Distance > NearZero)
        {
            MoveToTarget(playerId, context, target);
            CheckDiscCollision(state, target, context);
        }
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

            // Calculate partial rotation offset
            var bounceDistPercentage = collisionPosAmount / target.Distance;
            var bounceRotAmount = bounceDistPercentage * target.AngleOffset;

            // Reflect rotation against wall + apply partial rotation offset
            target.Heading = Vector2.Reflect(nextPos - trans.Position, collisionWall.Normal).Rotate(bounceRotAmount).normalized;
            target.AngleOffset -= bounceRotAmount;
            trans.Rotation = Vector2.up.SignedAngle(target.Heading);
        }
        else
        {
            target.Distance = 0f;
            target.AngleOffset = 0f;
            trans.Position = nextPos;
            trans.Rotation = nextRot;
        }
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
