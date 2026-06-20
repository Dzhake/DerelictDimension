namespace DerelictDimension.ECS.Physics.Components;

public record struct MobileComponent : IComponent
{
    public Vector2 Velocity;
    public int SupportingEntityId = -1;

    //probably don't need this one?
    public float HighestPoint = float.MaxValue;

    public readonly bool InAir => SupportingEntityId < 0;
    public readonly bool Grounded => SupportingEntityId >= 0;

    public MobileComponent()
    {
    }
}
