using System;
using UnityEngine;

[Serializable]
public class PaddleConfig
{
    [SerializeField]
    private int numPaddles = 1;

    [SerializeField]
    private float radius = 0.09f;

    [SerializeField]
    private float forwardSpeed = 3.2f;

    [SerializeField]
    private float orbitSpeed = 3.2f;

    [SerializeField]
    private float wallSlideSpeed = 6f;

    [SerializeField]
    private float mass = 10f;

    [SerializeField]
    private float orbitRadius = 0.7f;

    [SerializeField]
    private float toOrbitDeltaSpeed = 8f;

    [SerializeField]
    private float fromOrbitDeltaSpeed = -15f;

    [SerializeField]
    private float discBounceFactor = 1.3f;

    public int NumPaddles { get { return this.numPaddles; } }
    public float Radius { get { return this.radius; } }
    public float ForwardSpeed { get { return this.forwardSpeed; } }
    public float OrbitSpeed { get { return this.orbitSpeed; } }
    public float WallSlideSpeed { get { return this.wallSlideSpeed; } }
    public float Mass { get { return this.mass; } }
    public float OrbitRadius { get { return this.orbitRadius; } }
    public float ToOrbitDeltaSpeed { get { return this.toOrbitDeltaSpeed; } }
    public float FromOrbitDeltaSpeed { get { return this.fromOrbitDeltaSpeed; } }
    public float DiscBounceFactor { get { return this.discBounceFactor; } }
}
