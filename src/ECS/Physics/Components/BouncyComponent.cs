namespace DerelictDimension.ECS.Physics.Components;

public record struct BouncyComponent : IComponent
{
    public bool DieOnBounce = true;

    public BouncyComponent()
    {
    }
}
