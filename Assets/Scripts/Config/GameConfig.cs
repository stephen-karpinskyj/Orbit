using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
    [Serializable]
    public class GameSizeConfig
    {
        [SerializeField, Range(0f, 5f)]
        private float goalWidth = 2f;

        [SerializeField, Range(0f, 5f)]
        private float zoneOffset = 2.8f;

        public float GoalWidth { get { return this.goalWidth; } }
        public float ZoneOffset { get { return this.zoneOffset; } }
    }

    [SerializeField]
    private GameSizeConfig nonTutorialSize;

    [SerializeField]
    private GameSizeConfig tutorialSize;

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

    public GameSizeConfig Size { get { return GameConfig.InTutorialMode ? this.tutorialSize : this.nonTutorialSize; } }
    public Box PaddleBox { get { return this.paddleBox; } }
    public PaddleConfig Paddle { get { return this.paddle; } }
    public Box DiscBox { get { return this.discBox; } }
    public DiscConfig Disc { get { return this.disc; } }
    public ColourConfig Colours { get { return this.colours; } }

    public static PaddlePlayerController.ControlScheme ControlScheme
    {
        get { return (PaddlePlayerController.ControlScheme)PlayerPrefs.GetInt("Options.ControlScheme", (int)PaddlePlayerController.ControlScheme.FollowDisc); }
        set
        {
            PlayerPrefs.SetInt("Options.ControlScheme", (int)value);
            PlayerPrefs.Save();

            Debug.Log("Control scheme changed to " + value);
        }
    }

    public static bool InTutorialMode
    {
        get { return PlayerPrefs.GetInt("Options.InTutorialMode", 0) == 1; }
        set
        {
            PlayerPrefs.SetInt("Options.InTutorialMode", value ? 1 : 0);
            PlayerPrefs.Save();

            Debug.Log("In tutorial mode changed to " + value);
        }
    }
}
