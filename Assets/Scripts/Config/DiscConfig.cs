using System;
using UnityEngine;

[Serializable]
public class DiscConfig
{
    [Serializable]
    public class DiscSizeConfig
    {
        [SerializeField]
        private float radius = 0.1667f;

        [SerializeField]
        private float mass = 20f;

        public float Radius { get { return this.radius; } }
        public float Mass { get { return this.mass; } }
    }

    [SerializeField]
    private DiscSizeConfig nonTutorialSize;

    [SerializeField]
    private DiscSizeConfig tutorialSize;

    [SerializeField]
    private float maxSpeed = 5f;

    [SerializeField]
    private float dragFactor = 0.992f;

    [SerializeField]
    private float wallBounceFactor = 1.5f;

    public DiscSizeConfig Size { get { return GameConfig.InTutorialMode ? this.tutorialSize : this.nonTutorialSize; } }
    public float MaxSpeed { get { return this.maxSpeed; } }
    public float DragFactor { get { return this.dragFactor; } }
    public float WallBounceFactor { get { return this.wallBounceFactor; } }
}
