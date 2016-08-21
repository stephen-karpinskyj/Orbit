using System.Collections.Generic;
using UnityEngine;

public class GameView : MonoBehaviour
{
    [SerializeField]
    private Camera gameCamera;

    [SerializeField]
    private Transform rootTrans;

    [SerializeField]
    private DiscView discPrefab;

    [SerializeField]
    private PaddleView paddlePrefab;

    private List<PaddleView> paddles = new List<PaddleView>();

    public DiscView Disc { get; private set; }

    private void Awake()
    {
        Debug.Assert(this.gameCamera, this);
        Debug.Assert(this.rootTrans, this);
        Debug.Assert(this.discPrefab, this);
        Debug.Assert(this.paddlePrefab, this);
    }

    public void Initialize(GameContext context)
    {
        this.Disc = GameObjectUtility.InstantiatePrefab(this.discPrefab, this.rootTrans);
        this.Disc.Initialize(context);

        for (var i = 0; i < context.Config.Paddle.NumPaddles; i++)
        {
            var view = GameObjectUtility.InstantiatePrefab(this.paddlePrefab, this.rootTrans);
            view.Initialize(i, context);
            this.paddles.Add(view);
        }

        this.UpdateColours(context);
    }

    public void UpdateTransforms(float time, GameContext context)
    {
        for (var i = 0; i < this.paddles.Count; i++)
        {
            this.paddles[i].UpdateTransform(time, context);
        }

        this.Disc.UpdateTransform(time, context);
    }

    public void UpdateColours(GameContext context)
    {
        var set = context.Config.Colours.CurrentSet;

        this.gameCamera.backgroundColor = set.BackgroundColour;

        foreach (var view in this.paddles)
        {
            view.UpdateColours(context);
        }
    }
}
