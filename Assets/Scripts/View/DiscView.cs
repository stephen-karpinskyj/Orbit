using UnityEngine;

public class DiscView : MonoBehaviour
{
    [SerializeField]
    private Transform rootTrans;

    private void Awake()
    {
        Debug.Assert(this.rootTrans, this);
    }

    public void UpdateTransform(GameContext context)
    {
        this.rootTrans.localPosition = context.State.Disc.Position;
    }
}
