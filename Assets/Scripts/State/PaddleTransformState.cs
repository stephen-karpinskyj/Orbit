﻿using System;
using UnityEngine;

[Serializable]
public class PaddleTransformState
{
    public Vector2 Position;

    [SerializeField]
    private float rotation;

    public float Rotation
    {
        get { return this.rotation; }
        set
        {
            this.rotation = value;

            /* FIXME: Wrapping makes it stutter when the wrap occurs 
            this.rotation = value % 360;

            if (this.rotation < 0)
            {
                this.rotation += 360;
            }
             */
        }
    }

    public PaddleTransformState Clone()
    {
        return this.MemberwiseClone() as PaddleTransformState;
    }

    public static PaddleTransformState Lerp(PaddleTransformState a, PaddleTransformState b, float t)
    {
        var lerped = new PaddleTransformState();
        lerped.Position = Vector2.Lerp(a.Position, b.Position, t);
        lerped.Rotation = Mathf.Lerp(a.Rotation, b.Rotation, t);
        return lerped;
    }
}