namespace DerelictDimension.ECS.Ai;

public record struct PlayerAi : IComponent
{
    public static readonly float NormalWalkingSpeed = 400;

    public float TargetVelocity = NormalWalkingSpeed;
    public float Acceleration = 3000;

    public PlayerAi()
    {
    }
}
