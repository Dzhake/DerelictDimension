using DerelictDimension.ECS.Physics.Components;
using Monod.ECS.DefaultComponents;
using Monod.MathModule;

namespace DerelictDimension.ECS.Physics.Collisions;

public class BounceCollision : ICollision
{
    public Entity BouncyEntity;

    public void Apply(EntityData mobileData, ref Vector2 movement, float timeRemaining, float collisionTime, ref MobileComponent mobile, ref Position2D mobilePos, ref MobileInfoComponent mobileInfo)
    {
        if (!mobileData.Has<BounceableComponent>()) return;
        ref var bounceable = ref mobileData.Get<BounceableComponent>();

        if (mobile.Velocity.Y <= 0) return;

        float gravity = Math.Max(PhysicsSystem.GravityAccel.Y, 0.0001f);
        float incomingVelocity = mobile.Velocity.Y;

        // H = v^2 / 2g
        float currentHeight = (incomingVelocity * incomingVelocity) / (2f * gravity);

        float targetHeight = Math.Clamp(currentHeight + bounceable.AddHeight, bounceable.MinHeight, bounceable.MaxHeight);

        // v = sqrt(2gH)
        float targetVelocity = (float)Math.Sqrt(2f * gravity * targetHeight);

        float bounceY = mobile.Velocity.Y > 0.001f ? targetVelocity / mobile.Velocity.Y : 0f;

        Vector2 bounce = new(0, bounceY);

        // TODO: i'm not sure if this is how the speed should be set. Might get very incosistent behaviour with different framerate.
        PhysicsSystem.ApplyBounce(ref movement, MathM.VectorUp, bounce);
        PhysicsSystem.ApplyBounce(ref mobile.Velocity, MathM.VectorUp, bounce);
    }
}
