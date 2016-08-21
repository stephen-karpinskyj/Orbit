using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
    [SerializeField, Range(0f, 5f)]
    private float goalWidth = 2f;

    [SerializeField, Range(0f, 5f)]
    private float zoneOffset = 2.5f;

    [SerializeField]
    private Box paddleBox;

    [SerializeField]
    private PaddleConfig paddle;

    [SerializeField]
    private Box discBox;

    [SerializeField]
    private DiscConfig disc;

    [SerializeField]
    private ColourConfig colours;

    public float GoalWidth { get { return this.goalWidth; } }
    public float ZoneOffset { get { return this.zoneOffset; } }
    public Box PaddleBox { get { return this.paddleBox; } }
    public PaddleConfig Paddle { get { return this.paddle; } }
    public Box DiscBox { get { return this.discBox; } }
    public DiscConfig Disc { get { return this.disc; } }
    public ColourConfig Colours { get { return this.colours; } }
}
