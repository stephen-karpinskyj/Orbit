using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameState
{
    [SerializeField]
    private DiscState disc;

    [SerializeField]
    private PaddleState[] paddles;

    public float PrevTime;
    public float Time;

    public DiscState Disc { get { return this.disc; } }
    public IEnumerable<PaddleState> Paddles { get { return this.paddles; } }

    private GameState() { }

    public GameState(GameConfig config)
    {
        this.disc = new DiscState();

        this.paddles = new PaddleState[config.Paddle.NumPaddles];
        for (var i = 0; i < this.paddles.Length; i++)
        {
            this.paddles[i] = new PaddleState(i);
        }
    }

    public PaddleState GetPaddle(int playerId)
    {
        Debug.Assert(playerId >= 0 && playerId < this.paddles.Length);

        return this.paddles[playerId];
    }
}
