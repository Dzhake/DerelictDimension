using Friflo.Engine.ECS;

namespace DerelictDimension.ECS.Physics;

public struct ActorComponent : IComponent
{
    public Vector2 Velocity;
    public RectangleF Hitbox;
    public int RidingEntityId = -1;
    public readonly bool InAir => RidingEntityId < 0;

    public ActorComponent()
    {

    }
}

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

public struct SolidComponent : IComponent
{
    public RotatedRectangle Hitbox;
}

public struct MovingSolidComponent : IComponent
{
    public Vector2 Velocity;
    public bool Pushable;
}
