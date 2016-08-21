using System;
using UnityEngine;

[Serializable]
public class DiscConfig
{
    [SerializeField]
    private float radius = 0.09f;

    [SerializeField]
    private float mass = 10f;

    [SerializeField]
    private float maxSpeed = 6f;

    [SerializeField]
    private float dragFactor = 0.7f;

    [SerializeField]
    private float wallBounceFactor = 1.3f;

    public float Radius { get { return this.radius; } }
    public float Mass { get { return this.mass; } }
    public float MaxSpeed { get { return this.maxSpeed; } }
    public float DragFactor { get { return this.dragFactor; } }
    public float WallBounceFactor { get { return this.wallBounceFactor; } }
}
