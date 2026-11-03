namespace DerelictDimension.ECS.Ai.Damage;

public record struct DamagerComponent : IComponent
{
    public float Damage;
    public int OwnerId;
}
