using DerelictDimension.ECS.Rewinding;

namespace DerelictDimension.ECS.Physics.Components;

public record struct MortalComponent : IComponent
{
    public bool Dead;
    public bool DiesToLethal = true;

    public MortalComponent()
    {
    }

    public override readonly string ToString() => $"Dead: {Dead}";


    public void Kill(int entityId, ref EntityData entityData)
    {
        Rewind.StoreComponentUpdated(entityId, ref this);
        Dead = true;
        if (entityData.Has<HitboxComponent>())
        {
            ref HitboxComponent hitbox = ref entityData.Get<HitboxComponent>();
            Rewind.StoreComponentUpdated(entityId, ref hitbox);
            hitbox.Collidable = false;
        }
    }
}
