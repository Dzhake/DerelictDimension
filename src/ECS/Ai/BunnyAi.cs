namespace DerelictDimension.ECS.Ai;

public record struct BunnyAi : IComponent
{
    public float TimeGroundedSinceJump;
    public float SmallJumpsDone;
}
