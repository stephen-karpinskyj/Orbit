using UnityEngine;

public class DiscView : MonoBehaviour
{
    [SerializeField]
    private Transform rootTrans;

    [SerializeField]
    private TrailRenderer trail;

    private void Awake()
    {
        Debug.Assert(this.rootTrans, this);
        Debug.Assert(this.trail, this);
    }

    public void Initialize(GameContext context)
    {
        this.UpdateTransform(0f, context);
    }

    public void EnableTrail(bool enable)
    {
        if (enable)
        {
            this.trail.Clear();
        }

        this.trail.enabled = enable;
    }

    public void UpdateTransform(float time, GameContext context)
    {
        var state = context.State.Disc;

        var trans = state.CalculateTransformAtTime(time, context);

        this.rootTrans.localPosition = trans.Position;
        this.rootTrans.localEulerAngles = Vector3.forward * trans.Rotation;
    }
}
