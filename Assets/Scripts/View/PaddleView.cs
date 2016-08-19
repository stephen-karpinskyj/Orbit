using UnityEngine;

public class PaddleView : MonoBehaviour
{
    [SerializeField]
    private Transform rootTrans;

    [SerializeField]
    private Transform rotationTrans;

    [SerializeField]
    private TrailRenderer trailRenderer;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private float directionVisualUpdateSpeed = 1400f;

    private int playerId;

    private float currRot;
    private float targetRot;

    private void Awake()
    {
        Debug.Assert(this.rootTrans, this);
        Debug.Assert(this.rotationTrans, this);
        Debug.Assert(this.trailRenderer, this);
        Debug.Assert(this.spriteRenderer, this);
    }

    private void Update()
    {
        if (Mathf.Approximately(this.currRot, this.targetRot))
        {
            return;
        }

        this.currRot += this.directionVisualUpdateSpeed * Time.deltaTime * Mathf.Sign(this.targetRot - this.currRot);
        this.currRot = Mathf.Clamp(this.currRot, 0, 90);

        this.rotationTrans.localEulerAngles = Vector3.forward * this.currRot;
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
        this.spriteRenderer.material.color = set.ForegroundColour;

        this.UpdateDirection(context);
    }

    private void UpdateDirection(GameContext context)
    {
        var state = context.State.GetPaddle(this.playerId);

        this.targetRot = state.Direction == Direction.CW ? 0 : 90;
    }
}
