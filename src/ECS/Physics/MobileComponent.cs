namespace DerelictDimension.ECS.Physics;

public struct MobileComponent : IComponent
{
    public Vector2 Velocity;
    public int RidingEntityId = -1;
    public readonly bool InAir => RidingEntityId < 0;
    public readonly bool Grounded => RidingEntityId >= 0;
    public bool AffectedByGravity = true;

    public MobileComponent()
    {
    }
}
