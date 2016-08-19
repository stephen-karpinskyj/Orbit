public static class DiscController
{
    public static TransformTarget StepTarget(float deltaTime, GameContext context)
    {
        var state = context.State.Disc;
        var target = new TransformTarget();

        state.Speed *= context.Config.Disc.DragFactor;

        StepTransform(deltaTime, context, target);

        return target;
    }

    private static void StepTransform(float deltaTime, GameContext context, TransformTarget target)
    {
        var state = context.State.Disc;

        var angleOffset = 0f;

        target.Heading = state.Heading;
        target.Distance = deltaTime * state.Speed;
        target.AngleOffset = angleOffset;
    }
}
