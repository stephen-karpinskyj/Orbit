using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
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

    public Box PaddleBox { get { return this.paddleBox; } }
    public PaddleConfig Paddle { get { return this.paddle; } }
    public Box DiscBox { get { return this.discBox; } }
    public DiscConfig Disc { get { return this.disc; } }
    public ColourConfig Colours { get { return this.colours; } }
}
