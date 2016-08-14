using System;
using UnityEngine;

[Serializable]
public class PaddleState
{
    public enum MovementMode
    {
        Normal = 0,
        Orbiting,
        WallSliding,
    }

    public readonly int PlayerId;

    private PaddleTransformState prevTransform;
    public PaddleTransformState Transform;

    public float Speed;
    public bool IsTapping;
    public MovementMode Mode;
    public Direction Direction;

    public float PercentOrbit;
    public Vector2 OrbitOrigin;

    public Line SlideWall;
    public bool HasTappedDownWhileWallSliding;

    public PaddleState(int playerId)
    {
        this.PlayerId = playerId;
        this.Transform = new PaddleTransformState();
        this.StartNextFrame();
    }

    public void StartNextFrame()
    {
        this.prevTransform = this.Transform.Clone();
    }

    public PaddleTransformState CalculateTransformAtTime(float time, GameContext context)
    {
        var timeDiff = context.State.Time - context.State.PrevTime;

        if (Mathf.Approximately(timeDiff, 0f)) 
        {
            return this.Transform.Clone();
        }
        
        var t = (time - context.State.PrevTime) / timeDiff;
        return PaddleTransformState.Lerp(this.prevTransform, this.Transform, t);
    }
}
