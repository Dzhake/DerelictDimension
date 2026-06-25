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

        bool hasMobile = data.Has<MobileComponent>();
        bool hasMobileInfo = data.Has<MobileInfoComponent>();
        if (!hasMobile)
        {
            MobileComponent defaultMobile = new() { Velocity = new(Speed, 0) };
            Rewind.StoreComponentBeforeAdd<MobileComponent>(entity.Id);
            cb.AddComponent(data.Id, defaultMobile);
        }

        if (!hasMobileInfo)
        {
            MobileInfoComponent defaultMobileInfo = new(flipOnEdge: FlipOnEdge, restitution: new(1, 0.2f), restitutionMinimumResultingVelocity: new(0, 1), frictionMult: 0);
            Rewind.StoreComponentBeforeAdd<MobileInfoComponent>(entity.Id);
            cb.AddComponent(data.Id, defaultMobileInfo);
        }

        if (!hasMobile || !hasMobileInfo) return;

        ref var mobile = ref data.Get<MobileComponent>();
        if (mobile.Grounded)
        {
            Rewind.StoreComponentUpdated(entity.Id, ref mobile);
            mobile.Velocity.X = Math.Sign(mobile.Velocity.X) * Speed;
        }
    }
}
