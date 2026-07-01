namespace DerelictDimension.ECS.Physics.Components;

public record struct CannonComponent : IComponent
{
    public float TimeSinceShooting;
    public Vector2[] Points;

}
