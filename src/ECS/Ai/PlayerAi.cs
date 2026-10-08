namespace DerelictDimension.ECS.Ai;

public record struct PlayerAi : IComponent
{
    public float TargetVelocity = 400;
    public float Acceleration = 3000;

    public PlayerAi()
    {
    }
}
