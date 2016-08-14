using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
    [SerializeField]
    private TableConfig table;

    [SerializeField]
    private PaddleConfig paddle;

    [SerializeField]
    private ColourConfig colours;

    public TableConfig Table { get { return this.table; } }
    public PaddleConfig Paddle { get { return this.paddle; } }
    public ColourConfig Colours { get { return this.colours; } }
}
