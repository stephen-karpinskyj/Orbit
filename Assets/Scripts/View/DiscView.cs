using UnityEngine;

public class DiscView : MonoBehaviour
{
    [SerializeField]
    private Transform rootTrans;

    private void Awake()
    {
        Debug.Assert(this.rootTrans, this);
    }

    public void Initialize(GameContext context)
    {
        this.UpdateTransform(0f, context);
    }

    public void UpdateTransform(float time, GameContext context)
    {
        var state = context.State.Disc;

        var trans = state.CalculateTransformAtTime(time, context);

        this.rootTrans.localPosition = trans.Position;
        this.rootTrans.localEulerAngles = Vector3.forward * trans.Rotation;
    }
}
