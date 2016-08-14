using UnityEngine;

public class PaddleView : MonoBehaviour
{
    [SerializeField]
    private Transform rootTrans;

    [SerializeField]
    private TrailRenderer trailRenderer;

    [SerializeField]
    private SpriteRenderer[] overlaySpriteRenderers;

    [SerializeField]
    private SpriteRenderer cwSpriteRenderer;

    [SerializeField]
    private SpriteRenderer ccwSpriteRenderer;

    private int playerId;

    private void Awake()
    {
        Debug.Assert(this.rootTrans, this);

        Debug.Assert(this.trailRenderer, this);

        foreach (var s in this.overlaySpriteRenderers)
        {
            Debug.Assert(s, this);
        }

        Debug.Assert(this.cwSpriteRenderer, this);
        Debug.Assert(this.ccwSpriteRenderer, this);
    }

    public void Initialize(int playerId, GameContext context)
    {
        this.playerId = playerId;

        this.UpdateTransform(0f, context);
    }

    public void UpdateTransform(float time, GameContext context)
    {
        var state = context.State.GetPaddle(this.playerId);

        var trans = state.CalculateTransformAtTime(time, context);

        this.rootTrans.localPosition = trans.Position;
        this.rootTrans.localEulerAngles = Vector3.forward * trans.Rotation;

        this.UpdateDirection(context);
    }

    public void UpdateColours(GameContext context)
    {
        var set = context.Config.Colours.CurrentSet;

        this.trailRenderer.material.color = set.ForegroundColour;

        foreach (var s in this.overlaySpriteRenderers)
        {
            s.color = set.ForegroundColour;
        }

        this.UpdateDirection(context);
    }

    private void UpdateDirection(GameContext context)
    {
        var colourSet = context.Config.Colours.CurrentSet;
        var state = context.State.GetPaddle(this.playerId);

        this.cwSpriteRenderer.color = state.Direction == Direction.CW ? colourSet.HighlightColour : colourSet.ForegroundColour;
        this.ccwSpriteRenderer.color = state.Direction == Direction.CCW ? colourSet.HighlightColour : colourSet.ForegroundColour;
    }
}
