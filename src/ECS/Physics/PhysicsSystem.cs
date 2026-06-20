using DerelictDimension.ECS.Physics;
using DerelictDimension.ECS.Physics.Collisions;
using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;
using Monod.ECS.DefaultComponents;
using Monod.MathModule;
using Monod.TimeModule;
using System;

public class PhysicsSystem : BaseSystem
{
    public static readonly Vector2 GravityAccel = new(0, 1000);

    public ArchetypeQuery<MobileComponent, MobileInfoComponent> MobilesQuery;
    public ArchetypeQuery<SupportComponent> SupportsQuery;
    public ArchetypeQuery<BouncyComponent> BouncyQuery;
    public EntityStore Store;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        MobilesQuery = store.Query<MobileComponent, MobileInfoComponent>();
        SupportsQuery = store.Query<SupportComponent>();
        BouncyQuery = store.Query<BouncyComponent>();
        Store = store;
    }

    protected override void OnUpdateGroup()
    {
        float dt = Time.DeltaTime;

        var cb = Store.GetCommandBuffer();

        foreach (Entity mobileEnt in MobilesQuery.Entities)
        {
            var mobileData = mobileEnt.Data;
            if (!mobileData.Has<Transform2D>() || !mobileData.Has<HitboxComponent>()) continue;
            ref var mobile = ref mobileData.Get<MobileComponent>();
            ref var mobileInfo = ref mobileData.Get<MobileInfoComponent>();
            ref var mobileTransform = ref mobileData.Get<Transform2D>();
            ref var mobileHitbox = ref mobileData.Get<HitboxComponent>();
            bool temporaryTimeless = mobileData.Has<TemporaryTimeless>();
            bool isTimeless = mobileData.Has<TimelessComponent>();

            if (!Rewind.Active || isTimeless)
            {
                Rewind.Keep(mobileEnt, ref mobile);
                Rewind.Keep(mobileEnt, ref mobileTransform);

                // Apply forces
                if (mobile.Grounded)
                {
                    var supportEnt = Store.GetEntityById(mobile.SupportingEntityId);

                    if (!supportEnt.IsNull && supportEnt.HasComponent<SupportComponent>())
                    {
                        mobile.Velocity.X *= 1 + supportEnt.GetComponent<SupportComponent>().Friction * mobileInfo.FrictionMult;
                        if (Math.Abs(mobile.Velocity.X) < 0.01f) mobile.Velocity.X = 0;
                    }
                }

                if (mobileInfo.AffectedByGravity)
                    mobile.Velocity += GravityAccel * dt;

                AABB worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);

                //should probably check 'inside solid' in sweep check instead?
                /*
                foreach (Entity supportEnt in SupportsQuery.Entities)
                {
                    var supportData = supportEnt.Data;
                    if (!supportData.Has<Transform2D>() || !supportData.Has<HitboxComponent>() || !supportData.Has<SolidComponent>()) continue;

                    ref var supportPos = ref supportData.Get<Transform2D>();
                    ref SupportComponent supportC = ref supportData.Get<SupportComponent>();
                    AABB worldSupportHitbox = GetWorldHitbox(ref supportData.Get<HitboxComponent>(), ref supportPos);

                    if (Collide.CheckAABBToAABB(ref worldMobileHitbox, ref worldSupportHitbox, out Vector2 mtv))
                    {
                        mobileTransform.Value += mtv;
                        ApplyBounce(ref mobile.Velocity, mtv, GetRestitution(ref mobileInfo, ref supportC));
                        worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);
                    }
                }
                */

                MoveMobileEntity(mobile.Velocity * dt, mobileData);
            }

            if (mobileData.Has<TimelessComponent>() && !mobileData.Has<TemporaryTimeless>()) continue;
            bool shouldBeTempTimeless = false;
            if (mobile.SupportingEntityId != -1)
            {
                Entity supportingEnt = Store.GetEntityById(mobile.SupportingEntityId);
                var supportingData = supportingEnt.Data;
                ref var support = ref supportingData.Get<SupportComponent>();
                shouldBeTempTimeless = support.MakeTimeless;
            }
            if (shouldBeTempTimeless) MakeTimeless(mobileData, cb);
            else UnmakeTimeless(mobileData, cb);
        }
        cb.Playback();
    }

    private void MoveMobileEntity(Vector2 movement, EntityData mobileData, bool force = false)
    {
        ref var mobileHitbox = ref mobileData.Get<HitboxComponent>();
        ref var mobile = ref mobileData.Get<MobileComponent>();
        ref var mobileTransform = ref mobileData.Get<Transform2D>();
        if (!mobileHitbox.Collidable)
        {
            FreeMoveMobile(ref mobile, ref mobileTransform, movement);
            return;
        }
        ref var mobileInfo = ref mobileData.Get<MobileInfoComponent>();
        bool isBounceable = mobileData.Has<BounceableComponent>();

        AABB worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);

        int maxIterations = 5;
        float timeRemaining = 1.0f;

        for (int i = 0; i < maxIterations && timeRemaining > 0; i++)
        {
            Vector2 currentFrameMovement = movement * timeRemaining;
            AABB quickCheckHitbox = worldMobileHitbox.Union(worldMobileHitbox with { Center = worldMobileHitbox.Center + currentFrameMovement });

            float minCollisionTime = 1.0f;
            // find most recent collision
            ICollision? collision = FindCollision(ref movement, force, ref mobile, ref mobileInfo, isBounceable, ref worldMobileHitbox, ref currentFrameMovement, ref quickCheckHitbox, ref minCollisionTime);

            //apply collision
            if (collision != null)
            {
                collision.Apply(mobileData, ref movement, timeRemaining, minCollisionTime, ref mobile, ref mobileTransform, ref mobileInfo);
                timeRemaining -= timeRemaining * minCollisionTime;

                mobile.HighestPoint = Math.Min(mobile.HighestPoint, mobileTransform.Position.Y);
                worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);
            }
            else
            {
                FreeMoveMobile(ref mobile, ref mobileTransform, currentFrameMovement);
                timeRemaining = 0;
            }
        }
    }

    private static void FreeMoveMobile(ref MobileComponent mobile, ref Transform2D mobileTransform, Vector2 movement)
    {
        if (movement.Y != 0)
            mobile.SupportingEntityId = -1;
        mobileTransform.Position += movement;
        mobile.HighestPoint = Math.Min(mobile.HighestPoint, mobileTransform.Position.Y);
    }

    private ICollision? FindCollision(ref Vector2 movement, bool force, ref MobileComponent mobile, ref MobileInfoComponent mobileInfo, bool isBounceable, ref AABB worldMobileHitbox, ref Vector2 currentFrameMovement, ref AABB quickCheckHitbox, ref float minCollisionTime)
    {
        ICollision? collision = null;

        // collision with support/solid
        foreach (Entity supportEnt in SupportsQuery.Entities)
        {
            var supportData = supportEnt.Data;
            ref var supportC = ref supportData.Get<SupportComponent>();

            float collisionTime = CheckSweepCollision(ref worldMobileHitbox, ref quickCheckHitbox, currentFrameMovement, minCollisionTime, supportEnt, out Vector2 normal);
            if (collisionTime != 1)
            {
                bool supportIsSolid = supportData.Has<SolidComponent>();
                if (supportIsSolid || supportC.Normals.Matches(normal))
                {
                    minCollisionTime = collisionTime;
                    if (collision is not SupportCollision supportCollision) supportCollision = new();

                    supportCollision.Normal = normal;
                    supportCollision.SupportEntity = supportEnt;
                    collision = supportCollision;
                }
            }
        }

        // bouncing off bouncy entity
        if (isBounceable)
        {
            foreach (Entity bouncyEnt in BouncyQuery.Entities)
            {
                float collisionTime = CheckSweepCollision(ref worldMobileHitbox, ref quickCheckHitbox, currentFrameMovement, minCollisionTime, bouncyEnt, out Vector2 normal);
                if (normal == MathM.VectorUp && collisionTime != 1)
                {
                    minCollisionTime = collisionTime;
                    if (collision is not BounceCollision bounce) bounce = new();

                    bounce.BouncyEntity = bouncyEnt;
                    collision = bounce;
                }
            }
        }

        // flip on edge
        if (!force && mobileInfo.FlipOnEdge && currentFrameMovement.X != 0 && mobile.SupportingEntityId >= 0)
        {
            var supportingEntity = Store.GetEntityById(mobile.SupportingEntityId);
            var supportingData = supportingEntity.Data;
            if (supportingData.Has<HitboxComponent>() && supportingData.Has<Transform2D>())
            {
                ref var supportingHitbox = ref supportingData.Get<HitboxComponent>();
                ref var supportingPos = ref supportingData.Get<Transform2D>().Position;
                float collisionTime;
                if (movement.X > 0)
                {
                    float startRight = worldMobileHitbox.Right;
                    float platformRight = supportingHitbox.Value.Right + supportingPos.X;
                    float endRight = startRight + currentFrameMovement.X;
                    collisionTime = (platformRight - startRight) / (endRight - startRight);
                }
                else
                {
                    float startLeft = worldMobileHitbox.Left;
                    float platformLeft = supportingHitbox.Value.Left + supportingPos.X;
                    float endLeft = startLeft + currentFrameMovement.X;
                    collisionTime = (startLeft - platformLeft) / (startLeft - endLeft);
                }

                if (collisionTime >= 0 && collisionTime < minCollisionTime)
                {
                    minCollisionTime = collisionTime;
                    if (collision is not FlipCollision virtualCollision) virtualCollision = new();

                    virtualCollision.Normal = new(-Math.Sign(movement.X), 0);
                    virtualCollision.Restitution = new(1, 0);
                    collision = virtualCollision;
                }
            }
        }

        return collision;
    }

    private static float CheckSweepCollision(ref AABB worldMobileHitbox, ref AABB quickCheckHitbox, Vector2 movement, float minCollisionTime, Entity collisionEnt, out Vector2 normal)
    {
        normal = default;
        var data = collisionEnt.Data;
        if (!data.Has<Transform2D>() || !data.Has<HitboxComponent>()) return 1;

        ref var hitbox = ref data.Get<HitboxComponent>();
        if (!hitbox.Collidable) return 1;

        ref var bouncyPos = ref data.Get<Transform2D>();
        AABB worldHitbox = GetWorldHitbox(ref hitbox, ref bouncyPos);

        if (!Collide.QuickCheckAABBToAABB(ref quickCheckHitbox, ref worldHitbox)) return 1;

        if (Collide.SweptCheck(ref worldMobileHitbox, ref worldHitbox, ref movement, out float collisionTime, out normal) && collisionTime < minCollisionTime)
            return collisionTime;
        return 1;
    }

    public static void ApplyBounce(ref Vector2 vector, Vector2 collisionDirection, Vector2 bounce)
    {
        if (collisionDirection.X != 0)
        {
            if (Math.Abs(vector.X) < 0.1f)
                vector.X = 0;
            else
                vector.X *= -bounce.X;
        }

        if (collisionDirection.Y != 0)
        {
            if (Math.Abs(vector.Y) < 0.1f)
                vector.Y = 0;
            else
                vector.Y *= -bounce.Y;
        }
    }

    public static Vector2 GetRestitution(ref MobileInfoComponent mobileInfo, ref SupportComponent support)
    {
        Vector2 overrideRestitution = support.OverrideRestitution;
        return new(overrideRestitution.X >= 0 ? overrideRestitution.X : mobileInfo.Restitution.X, overrideRestitution.Y >= 0 ? overrideRestitution.Y : mobileInfo.Restitution.Y);
    }

    private void UnmakeTimeless(EntityData data, CommandBuffer cb)
    {
        if (data.Has<TemporaryTimeless>() && data.Has<TimelessComponent>()) cb.RemoveComponent<TimelessComponent>(data.Id);
    }

    private void MakeTimeless(EntityData data, CommandBuffer cb)
    {
        if (!data.Has<TimelessComponent>()) cb.AddComponent(data.Id, new TimelessComponent());
        if (!data.Has<TemporaryTimeless>()) cb.AddComponent(data.Id, new TemporaryTimeless());
    }

    public static AABB GetWorldHitbox(ref HitboxComponent hitbox, ref Transform2D transform)
    {
        float effectiveScaleX = transform.ScaleX * (transform.FlipX ? -1f : 1f);
        float effectiveScaleY = transform.ScaleY * (transform.FlipY ? -1f : 1f);

        float worldCenterX = transform.PosX + (hitbox.Value.CenterX * effectiveScaleX);
        float worldCenterY = transform.PosY + (hitbox.Value.CenterY * effectiveScaleY);

        // half sizes must be non-negative, scale does not, so we need to take absolute value of scale.
        float worldHalfWidth = hitbox.Value.HalfWidth * MathF.Abs(transform.ScaleX);
        float worldHalfHeight = hitbox.Value.HalfHeight * MathF.Abs(transform.ScaleY);

        return new AABB(worldCenterX, worldCenterY, worldHalfWidth, worldHalfHeight);
    }
}
