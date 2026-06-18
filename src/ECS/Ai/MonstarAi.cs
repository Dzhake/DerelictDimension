using DerelictDimension.ECS.Physics.Components;

namespace DerelictDimension.ECS.Ai;

public struct MonstarAi : IComponent, IAi
{
    public float Speed = 150;
    public bool FlipOnEdge = true;

    public MonstarAi()
    {
    }

    public void PostUpdate(Entity entity, EntityStore store, CommandBuffer cb) { }
    public void PreUpdate(Entity entity, EntityStore store, CommandBuffer cb)
    {
        var data = entity.Data;
        bool hasMobile = data.Has<MobileComponent>();
        bool hasMobileInfo = data.Has<MobileInfoComponent>();
        if (!hasMobile)
        {
            MobileComponent defaultMobile = new() { Velocity = new(Speed, 0) };
            cb.AddComponent(data.Id, defaultMobile);
        }

        if (!hasMobileInfo)
        {
            MobileInfoComponent defaultMobileInfo = new(flipOnEdge: FlipOnEdge, restitution: new(1, 0.2f), frictionMult: 0);
            cb.AddComponent(data.Id, defaultMobileInfo);
        }

        if (!hasMobile || !hasMobileInfo) return;

        ref var mobile = ref data.Get<MobileComponent>();
        if (mobile.InAir) mobile.Velocity.X = Math.Sign(mobile.Velocity.X) * Speed;
    }
}
