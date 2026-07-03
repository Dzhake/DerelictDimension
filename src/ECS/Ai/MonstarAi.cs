using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Monod.ECS.DefaultComponents;
using Monod.TimeModule;
using System;

namespace DerelictDimension.ECS.Ai;

public record struct MonstarAi : IComponent, IAi
{
    public float Speed = 150;
    public float RotationSpeedWhileDead = -MathF.PI;
    public bool FlipOnEdge = true;

    public MonstarAi()
    {
    }

    public void PostUpdate(Entity entity, EntityStore store, CommandBuffer cb) { }
    public void PreUpdate(Entity entity, EntityStore store, CommandBuffer cb)
    {
        var data = entity.Data;
        if (!Rewind.ShouldUpdateEntity(data)) return;
        if (!data.Has<Transform2D>()) return;
        ref var transform = ref data.Get<Transform2D>();

        if (data.Has<MortalComponent>())
        {
            ref var mortal = ref data.Get<MortalComponent>();
            if (mortal.Dead)
            {
                Rewind.StoreComponentUpdated(entity.Id, ref transform);
                transform.Rotation += RotationSpeedWhileDead * Time.DeltaTime;
            }
        }

        if (!data.Has<MobileComponent>() || !data.Has<MobileInfoComponent>()) return;

        ref var mobile = ref data.Get<MobileComponent>();
        if (mobile.Grounded)
        {
            Rewind.StoreComponentUpdated(entity.Id, ref mobile);
            bool faceLeft = mobile.Velocity.X < 0 || transform.FlipX;
            mobile.Velocity.X = (faceLeft ? -1 : 1) * Speed;
        }
    }
}
