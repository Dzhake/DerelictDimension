namespace DerelictDimension.ECS.Physics.Components;

public record struct MortalComponent : IComponent
{
    public bool Dead;

    public override readonly string ToString() => $"Dead: {Dead}";
}
