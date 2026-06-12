namespace DerelictDimension.ECS.Physics;

public struct PlayerControlledComponent : IComponent
{
    public float TargetXVel = 200;
    public float Acceleration = 2000;
    public float AirAcceleration = 600;
    public float JumpStrength = 500;

    public PlayerControlledComponent()
    {
    }
}
