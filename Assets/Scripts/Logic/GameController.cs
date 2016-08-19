using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameConfig config;

    [SerializeField]
    private GameView view;

    [SerializeField]
    private RectTransform canvasRect;

    [SerializeField]
    private int controllingPlayerId = 0;

    [SerializeField]
    private TransformState initialPaddleTransform;

    [SerializeField]
    private float timestep = 1 / 60f;

    [SerializeField]
    private int framerate = 60;

    private GameState state;
    private GameContext context;

    private void Start()
    {
        Debug.Assert(this.view, this);
        Debug.Assert(this.canvasRect, this);

        // Config
        var tableSize = this.CalculateTableSize();
        this.config.PaddleBox.Initialize(tableSize, config.Paddle.Radius);
        this.config.DiscBox.Initialize(tableSize, config.Disc.Radius);
        this.config.Colours.GenerateColourSet();

        // State
        this.state = new GameState(this.config);
        this.state.GetPaddle(0).Transform = this.initialPaddleTransform.Clone();

        // Context
        this.context = new GameContext(this.config, this.state);

        // View
        this.view.Initialize(this.context);
        this.view.UpdateColours(this.context);

        Application.targetFrameRate = this.framerate;
    }

    private void Update()
    {
        PaddlePlayerController.UpdateInput(this.controllingPlayerId, this.context);

        while (this.context.State.Time < Time.time)
        {
            this.context.State.PrevTime = this.context.State.Time;
            this.context.State.Time += this.timestep;

            foreach (var p in this.context.State.Paddles)
            {
                p.StartNextFrame();
                var paddleTarget = PaddlePlayerController.StepTarget(this.timestep, p.PlayerId, this.context);
                PaddlePhysics.ResolveTarget(p.PlayerId, this.context, paddleTarget);
            }

            this.context.State.Disc.StartNextFrame();
            var discTarget = DiscController.StepTarget(this.timestep, this.context);
            DiscPhysics.ResolveTarget(this.context, discTarget);
        }

        this.view.UpdateTransforms(Time.time, context);
    }

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return;
        }
        #endif

        foreach (var w in this.context.Config.PaddleBox.Walls)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(w.Start, w.End);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(w.Mid, w.Mid + w.Normal * 0.5f);
        }

        foreach (var p in this.context.State.Paddles)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(p.Transform.Position, this.context.Config.Paddle.Radius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(p.OrbitOrigin, this.context.Config.Paddle.OrbitRadius);
        }

        foreach (var w in this.context.Config.DiscBox.Walls)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(w.Start, w.End);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(w.Mid, w.Mid + w.Normal * 0.5f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.context.State.Disc.Transform.Position, this.context.Config.Disc.Radius);
    }

    private Vector2 CalculateTableSize()
    {
        var screenSize = this.canvasRect.sizeDelta;
        screenSize.x *= this.canvasRect.localScale.x;
        screenSize.y *= this.canvasRect.localScale.y;

        return screenSize;
    }
}
