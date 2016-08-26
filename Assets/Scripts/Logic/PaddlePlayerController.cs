using System;
using UnityEngine;

public static class PaddlePlayerController
{
    public enum ControlScheme
    {
        FollowDisc = 0,
        DualTap,
        TapToggle,
        Count,
    }

    public static void UpdateInput(int playerId, GameContext context)
    {
        var state = context.State.GetPaddle(playerId);

        var wasTapping = state.IsTapping;
        state.IsTapping = Input.anyKey;

        switch (GameConfig.ControlScheme)
        {
            case ControlScheme.FollowDisc:
            case ControlScheme.TapToggle:
            {
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
            break;

            case ControlScheme.DualTap:
            {
                if (state.IsTapping && state.Mode != PaddleState.MovementMode.WallSliding)
                {
                    #if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE)
                    var halfScreen = Screen.width / 2f;
                    var isLeftTapping = (Input.touchCount > 0 && Input.GetTouch(0).position.x < halfScreen) || (Input.touchCount > 1 && Input.GetTouch(1).position.x < halfScreen);
                    var isRightTapping = (Input.touchCount > 0 && Input.GetTouch(0).position.x >= halfScreen) || (Input.touchCount > 1 && Input.GetTouch(1).position.x >= halfScreen);
                    #else
                    var isLeftTapping = Input.GetMouseButton(0) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
                    var isRightTapping = Input.GetMouseButton(1) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
                    #endif

                    if (isLeftTapping)
                    {
                        state.Direction = Direction.CCW;
                        state.Mode = PaddleState.MovementMode.Orbiting;
                    }
                    else if (isRightTapping)
                    {
                        state.Direction = Direction.CW;
                        state.Mode = PaddleState.MovementMode.Orbiting;
                    }
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
            break;
        }

        if (GameConfig.ControlScheme == ControlScheme.TapToggle)
        {
            if (wasTapping && !state.IsTapping)
            {
                state.Direction = state.Direction.Opposite();
            }
        }
    }

    public static TransformTarget StepTarget(float deltaTime, int playerId, GameContext context)
    {
        var state = context.State.GetPaddle(playerId);
        var target = new TransformTarget();

        StepPercentOrbit(deltaTime, playerId, context);

        if (state.Mode == PaddleState.MovementMode.Normal)
        {
            UpdateOrbitDirection(playerId, context);
        }

        if (state.Mode != PaddleState.MovementMode.WallSliding)
        {
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
        if (GameConfig.ControlScheme == ControlScheme.FollowDisc)
        {
            var state = context.State.GetPaddle(playerId);
            var trans = state.Transform;

            var right = MathUtility.ToHeading(trans.Rotation - 90);
            var to = context.State.Disc.Transform.Position - trans.Position;
            var dot = Vector3.Dot(right, to);

            state.Direction = dot > 0f ? Direction.CW : Direction.CCW;
        }
    }

    private static void UpdateOrbitOrigin(int playerId, GameContext context)
    {
        var state = context.State.GetPaddle(playerId);
        var trans = state.Transform;

        var v = MathUtility.ToHeading(trans.Rotation);
        var offset = context.Config.Paddle.Size.OrbitRadius;

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

        /*if (state.Mode == PaddleState.MovementMode.WallSliding)
        {
            // Add forward
            {
                var forwardDir = MathUtility.ToHeading(trans.Rotation);
                var forwardDist = deltaTime * context.Config.Paddle.Size.WallSlideSpeed;
                var forwardPosOffset = forwardDir * forwardDist;

                posOffset += forwardPosOffset;
            }
        }
        else*/
        {
            // Add orbit
            {
                var orbitDist = deltaTime * context.Config.Paddle.Size.OrbitSpeed * state.PercentOrbit;
                var orbitDegrees = MathUtility.CircleArcDistanceToAngleOffset(orbitDist, context.Config.Paddle.Size.OrbitRadius);

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
                var forwardDist = deltaTime * context.Config.Paddle.Size.ForwardSpeed * (1 - state.PercentOrbit);
                var forwardPosOffset = forwardDir * forwardDist;

                posOffset += forwardPosOffset;
            }
        }

        target.Heading = posOffset.normalized;
        target.Distance = posOffset.magnitude;
        target.AngleOffset = angleOffset;
    }
}
