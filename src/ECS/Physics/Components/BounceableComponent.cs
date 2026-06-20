using Monod.Shared.Exceptions;

namespace DerelictDimension.ECS.Physics.Components;

public record struct BounceableComponent : IComponent
{
    public float MinHeight;
    public float MaxHeight;
    public float AddHeight;

    public BounceableComponent(float minHeight, float maxHeight, float addHeight)
    {
        if (minHeight > maxHeight) Guard.Exception("Bounceable component's max height can't be less than min height!");
        MinHeight = minHeight;
        MaxHeight = maxHeight;
        AddHeight = addHeight;
    }
}
