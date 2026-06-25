using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Monod.ECS.DefaultComponents;
using Monod.MathModule;
using System;

namespace DerelictDimension.ECS.Physics.Collisions;

public class BounceCollision : ICollision
{
    public Entity BouncyEntity;

    public void Apply(EntityData mobileData, ref Vector2 movement, float timeRemaining, float collisionTime, ref MobileComponent mobile, ref Transform2D mobileTransform, ref MobileInfoComponent mobileInfo)
    {
        float timeToMove = Math.Max(0.0f, collisionTime - MathM.Epsilon);
        Vector2 currentFrameMovement = movement * timeRemaining;
        mobileTransform.Position += currentFrameMovement * timeToMove;
        if (currentFrameMovement.Y != 0)
            mobile.SupportingEntityId = -1;

        var bouncyData = BouncyEntity.Data;
        if (!mobileData.Has<BounceableComponent>() || !bouncyData.Has<BouncyComponent>()) return;
        ref var bouncy = ref bouncyData.Get<BouncyComponent>();
        ref var bounceable = ref mobileData.Get<BounceableComponent>();

        if (mobile.Velocity.Y <= 0) return;

        float gravity = Math.Max(PhysicsSystem.GravityAccel.Y, 0.0001f);
        float incomingVelocity = mobile.Velocity.Y;

        // H = v^2 / 2g
        float currentHeight = (incomingVelocity * incomingVelocity) / (2f * gravity);

        float targetHeight = Math.Clamp(currentHeight + bounceable.AddHeight, bounceable.MinHeight, bounceable.MaxHeight);

        // v = sqrt(2gH)
        float targetVelocity = MathF.Sqrt(2f * gravity * targetHeight);

        float bounceY = mobile.Velocity.Y > 0.001f ? targetVelocity / mobile.Velocity.Y : 0f;

        Vector2 bounce = new(0, bounceY);

        if (bouncy.DieOnBounce && bouncyData.Has<MortalComponent>())
        {
            ref var mortal = ref bouncyData.Get<MortalComponent>();
            Rewind.StoreComponentUpdated(BouncyEntity.Id, ref mortal);
            mortal.Dead = true;
            if (bouncyData.Has<HitboxComponent>())
            {
                ref HitboxComponent hitbox = ref bouncyData.Get<HitboxComponent>();
                Rewind.StoreComponentUpdated(BouncyEntity.Id, ref hitbox);
                hitbox.Collidable = false;
            }
            if (bouncyData.Has<MobileComponent>())
            {
                ref var bouncyMobile = ref bouncyData.Get<MobileComponent>();
                Rewind.StoreComponentUpdated(BouncyEntity.Id, ref bouncyMobile);
                bouncyMobile.Velocity = (bouncyMobile.Velocity + mobile.Velocity) / 2;
            }
        }

        // TODO (probably fps-based bug, medium priority): i'm not sure if this is how the speed should be set, because it looks like goal height is based only on speed, so speed+movement will be higher than intended. Might get very incosistent behaviour with different framerate.
        PhysicsSystem.ApplyBounce(ref movement, MathM.VectorUp, bounce);
        PhysicsSystem.ApplyBounce(ref mobile.Velocity, MathM.VectorUp, bounce);
    }
}
