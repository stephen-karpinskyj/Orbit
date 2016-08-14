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
    private float speed = 4f;

    [SerializeField]
    private float orbitRadius = 0.7f;

    [SerializeField]
    private float toOrbitDeltaSpeed = 8f;

    [SerializeField]
    private float fromOrbitDeltaSpeed = -15f;

    public int NumPaddles { get { return this.numPaddles; } }
    public float Radius { get { return this.radius; } }
    public float Speed { get { return this.speed; } }
    public float OrbitRadius { get { return this.orbitRadius; } }
    public float ToOrbitDeltaSpeed { get { return this.toOrbitDeltaSpeed; } }
    public float FromOrbitDeltaSpeed { get { return this.fromOrbitDeltaSpeed; } }
}
