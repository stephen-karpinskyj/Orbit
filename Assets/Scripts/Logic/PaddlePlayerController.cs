using System;
using UnityEngine;

public static class PaddlePlayerController
{
    public static void UpdateInput(int playerId, GameContext context)
    {
        var state = context.State.GetPaddle(playerId);

        state.IsTapping = Input.anyKey;

        if (state.IsTapping && state.Mode != PaddleState.MovementMode.WallSliding)
        {
            state.Mode = PaddleState.MovementMode.Orbiting;
        }

        if (!state.IsTapping && state.Mode == PaddleState.MovementMode.Orbiting)
        {
            state.Mode = PaddleState.MovementMode.Normal;
        }

        if (!state.IsTapping && state.Mode == PaddleState.MovementMode.WallSliding)
        {
            state.Mode = PaddleState.MovementMode.Normal;
        }
    }

    public static TransformTarget StepTarget(float deltaTime, int playerId, GameContext context)
    {
        var state = context.State.GetPaddle(playerId);
        var target = new TransformTarget();

        StepPercentOrbit(deltaTime, playerId, context);

        if (state.Mode != PaddleState.MovementMode.WallSliding)
        {
            if (state.Mode == PaddleState.MovementMode.Normal)
            {
                UpdateOrbitDirection(playerId, context);
            }

            UpdateOrbitOrigin(playerId, context);
        }

        StepTransform(deltaTime, playerId, context, target);

        return target;
    }

    private static void StepPercentOrbit(float deltaTime, int playerId, GameContext context)
    {
        var state = context.State.GetPaddle(playerId);

        if (state.Mode == PaddleState.MovementMode.WallSliding)
        {
            state.PercentOrbit = 0f;
        }
        else
        {
            var mag = state.Mode == PaddleState.MovementMode.Orbiting ? context.Config.Paddle.ToOrbitDeltaSpeed : context.Config.Paddle.FromOrbitDeltaSpeed;
            var delta = mag * deltaTime;
            state.PercentOrbit = Mathf.Clamp01(state.PercentOrbit + delta);
        }
    }

    private static void UpdateOrbitDirection(int playerId, GameContext context)
    {
        var state = context.State.GetPaddle(playerId);
        var trans = state.Transform;

        var right = MathUtility.ToHeading(trans.Rotation - 90);
        var to = context.State.Disc.Transform.Position - trans.Position;
        var dot = Vector3.Dot(right, to);

        state.Direction = dot > 0f ? Direction.CW : Direction.CCW;
    }

    private static void UpdateOrbitOrigin(int playerId, GameContext context)
    {
        var state = context.State.GetPaddle(playerId);
        var trans = state.Transform;

        var v = MathUtility.ToHeading(trans.Rotation);
        var offset = context.Config.Paddle.OrbitRadius;

        if (state.Direction == Direction.CW)
        {
            offset *= -1;
        }

        // Based on: http://answers.unity3d.com/questions/564166/how-to-find-perpendicular-line-in-2d.html
        state.OrbitOrigin = trans.Position + new Vector2(-v.y, v.x) / Mathf.Sqrt(Mathf.Pow(v.x, 2) + Mathf.Pow(v.y, 2)) * offset;
    }

    private static void StepTransform(float deltaTime, int playerId, GameContext context, TransformTarget target)
    {
        var state = context.State.GetPaddle(playerId);
        var trans = state.Transform;

        var posOffset = Vector2.zero;
        var angleOffset = 0f;

        if (state.Mode == PaddleState.MovementMode.WallSliding)
        {
            // Add forward
            {
                var forwardDir = MathUtility.ToHeading(trans.Rotation);
                var forwardDist = deltaTime * context.Config.Paddle.WallSlideSpeed;
                var forwardPosOffset = forwardDir * forwardDist;

                posOffset += forwardPosOffset;
            }
        }
        else
        {
            // Add orbit
            {
                var orbitDist = deltaTime * context.Config.Paddle.OrbitSpeed * state.PercentOrbit;
                var orbitDegrees = MathUtility.CircleArcDistanceToAngleOffset(orbitDist, context.Config.Paddle.OrbitRadius);

                if (state.Direction == Direction.CW)
                {
                    orbitDegrees *= -1;
                }

                var orbitDest = trans.Position.RotateAround(state.OrbitOrigin, orbitDegrees);
                var orbitPosOffset = (orbitDest - trans.Position);

                posOffset += orbitPosOffset;
                angleOffset += orbitDegrees;
            }

            // Add forward
            {
                var forwardDir = MathUtility.ToHeading(trans.Rotation);
                var forwardDist = deltaTime * context.Config.Paddle.ForwardSpeed * (1 - state.PercentOrbit);
                var forwardPosOffset = forwardDir * forwardDist;

                posOffset += forwardPosOffset;
            }
        }

        target.Heading = posOffset.normalized;
        target.Distance = posOffset.magnitude;
        target.AngleOffset = angleOffset;
    }
}
