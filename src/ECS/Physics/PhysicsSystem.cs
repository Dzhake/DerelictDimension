using DerelictDimension.ECS.Physics.Collisions;
using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;
using Monod.ECS.DefaultComponents;
using Monod.MathModule;
using Monod.TimeModule;
using System;

namespace DerelictDimension.ECS.Physics;

public class PhysicsSystem : BaseSystem
{
    public static readonly Vector2 GravityAccel = new(0, 1000);

    public ArchetypeQuery<MobileComponent, MobileInfoComponent> MobilesQuery;
    public ArchetypeQuery<SupportComponent> SupportsQuery;
    public ArchetypeQuery SolidsQuery;
    public ArchetypeQuery<BouncyComponent> BouncyQuery;
    public ArchetypeQuery<LethalComponent> LethalsQuery;
    public EntityStore Store;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        MobilesQuery = store.Query<MobileComponent, MobileInfoComponent>();
        SupportsQuery = store.Query<SupportComponent>();
        BouncyQuery = store.Query<BouncyComponent>();
        SolidsQuery = store.Query().AllTags(Tags.Get<SolidTag>());
        LethalsQuery = store.Query<LethalComponent>();
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
            bool isTemporaryTimeless = mobileData.Has<TemporaryTimeless>();
            bool isTimeless = mobileData.Has<TimelessComponent>();

            if (!Rewind.Active || isTimeless)
            {
                Rewind.StoreComponentUpdated(mobileEnt.Id, ref mobile);
                Rewind.StoreComponentUpdated(mobileEnt.Id, ref mobileTransform);

                // Apply forces
                if (mobile.Grounded)
                {
                    var supportEnt = Store.GetEntityByPid(mobile.SupportingEntityPid);

                    if (!supportEnt.IsNull && supportEnt.HasComponent<SupportComponent>())
                    {
                        mobile.Velocity.X *= 1 + (supportEnt.GetComponent<SupportComponent>().Friction * mobileInfo.FrictionMult);
                        if (Math.Abs(mobile.Velocity.X) < 0.01f) mobile.Velocity.X = 0;
                    }
                }

                if (mobileInfo.AffectedByGravity)
                    mobile.Velocity += GravityAccel * dt;

                MoveMobileEntity(mobile.Velocity * dt, mobileData);
            }

            if (isTimeless && !isTemporaryTimeless) continue;
            bool shouldBeTempTimeless = false;
            if (mobile.SupportingEntityPid != -1)
            {
                Entity supportingEnt = Store.GetEntityByPid(mobile.SupportingEntityPid);
                var supportingData = supportingEnt.Data;
                ref var support = ref supportingData.Get<SupportComponent>();
                shouldBeTempTimeless = support.MakeTimeless;
            }

            if (shouldBeTempTimeless) MakeTimeless(mobileData, cb);
            else UnmakeTimeless(mobileData, cb);
        }
        cb.Playback();
    }

    /// <summary>
    /// Try to move the specified <paramref name="mobileData"/> the given <paramref name="movement"/>.
    /// </summary>
    /// <param name="movement"></param>
    /// <param name="mobileData"></param>
    /// <param name="force"></param>
    /// <returns>Whether entity successfully moved without collisions.</returns>
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
        bool isSupport = mobileData.Has<SupportComponent>();

        AABB worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);

        int maxIterations = 5;
        float timeRemaining = 1.0f;

        for (int i = 0; i < maxIterations && timeRemaining > 0; i++)
        {
            Vector2 currentFrameMovement = movement * timeRemaining;
            worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);
            AABB quickCheckHitbox = worldMobileHitbox.Union(worldMobileHitbox with { Center = worldMobileHitbox.Center + currentFrameMovement });

            float minCollisionTime = 1.0f;
            ICollision? collision = FindCollision(ref mobileData, ref movement, force, ref mobile, ref mobileInfo, ref currentFrameMovement, ref quickCheckHitbox, ref minCollisionTime);
            AABB worldHitboxBeforeStep = worldMobileHitbox;
            Vector2 movedThisStep;

            if (collision != null)
            {
                collision.Apply(mobileData, ref movement, timeRemaining, minCollisionTime, ref mobile, ref mobileTransform, ref mobileInfo);
                timeRemaining -= timeRemaining * minCollisionTime;

                movedThisStep = currentFrameMovement * minCollisionTime;
                mobile.HighestPoint = Math.Min(mobile.HighestPoint, mobileTransform.Position.Y);
            }
            else
            {
                FreeMoveMobile(ref mobile, ref mobileTransform, currentFrameMovement);
                timeRemaining = 0;
                movedThisStep = currentFrameMovement;
            }


            if (isSupport && movedThisStep != Vector2.Zero)
            {
                worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);
                PushMobiles(mobileData, ref worldHitboxBeforeStep, ref worldMobileHitbox, movedThisStep);
            }
        }
    }

    private void PushMobiles(EntityData supportData, ref AABB supportOriginalHitbox, ref AABB supportNewHitbox, Vector2 movedThisStep)
    {
        ref var support = ref supportData.Get<SupportComponent>();
        Vector2 relativeMovement = -movedThisStep;
        AABB supportUnion = supportOriginalHitbox.Union(supportNewHitbox);
        bool isSolid = supportData.Tags.Has<SolidTag>();

        foreach (Entity mobileEnt in MobilesQuery.Entities)
        {
            if (mobileEnt.Id == supportData.Id) continue;

            var mobileData = mobileEnt.Data;

            // important weakness: this physics system doesn't support entities with 'support' Component pushing each other while moving. Just colliding works fine.
            if (mobileData.Has<SupportComponent>()) continue;

            if (!mobileData.Has<HitboxComponent>() || !mobileData.Has<Transform2D>()) continue;
            ref var mobileHitbox = ref mobileData.Get<HitboxComponent>();
            if (!mobileHitbox.Collidable) continue;

            ref var mobileC = ref mobileData.Get<MobileComponent>();
            ref var mobileTransform = ref mobileData.Get<Transform2D>();

            if (mobileC.SupportingEntityPid == Store.IdToPid(mobileData.Id))
            {
                MoveMobileEntity(movedThisStep, mobileData, true);
                if (!IsAlive(ref mobileData)) continue;
                mobileTransform.Position.Y = supportNewHitbox.Top - mobileHitbox.Value.HalfHeight - MathM.Epsilon;
                AABB mobileNewHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);
                if (IsCrushed(ref mobileNewHitbox))
                    TryKillMortal(mobileEnt, ref mobileData);

                mobileC.SupportingEntityPid = Store.IdToPid(mobileData.Id);
                continue;
            }

            AABB worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);

            if (!Collide.QuickCheckAABBToAABB(ref supportUnion, ref worldMobileHitbox)) continue;

            if (Collide.SweptCheck(ref worldMobileHitbox, ref supportOriginalHitbox, ref relativeMovement, out float collisionTime, out Vector2 normal) && collisionTime >= 0.0f && collisionTime < 1.0f && (isSolid || support.Normals.Matches(normal)))
            {
                Vector2 pushAmount = movedThisStep * MathF.Max(0f, 1.0f - collisionTime);

                MoveMobileEntity(pushAmount, mobileData, true);
                if (!IsAlive(ref mobileData)) continue;
                AABB mobileNewHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);
                Vector2 snapOffset = Vector2.Zero;

                if (normal.X > 0.5f) snapOffset.X = supportNewHitbox.Right - mobileNewHitbox.Left + MathM.Epsilon;
                else if (normal.X < -0.5f) snapOffset.X = supportNewHitbox.Left - mobileNewHitbox.Right - MathM.Epsilon;

                if (normal.Y > 0.5f) snapOffset.Y = supportNewHitbox.Bottom - mobileNewHitbox.Top + MathM.Epsilon;
                else if (normal.Y < -0.5f) snapOffset.Y = supportNewHitbox.Top - mobileNewHitbox.Bottom - MathM.Epsilon;

                mobileTransform.Position += snapOffset;
                mobileNewHitbox = GetWorldHitbox(ref mobileHitbox, ref mobileTransform);

                if (IsCrushed(ref mobileNewHitbox))
                    TryKillMortal(mobileEnt, ref mobileData);

                if (normal == MathM.VectorUp)
                    mobileC.SupportingEntityPid = Store.IdToPid(mobileData.Id);
            }
        }
    }

    private void TryKillMortal(Entity mobileEnt, ref EntityData mobileData)
    {
        if (!mobileData.Has<MortalComponent>()) return;
        mobileData.Get<MortalComponent>().Kill(mobileData.Id, ref mobileData);
    }

    private bool IsCrushed(ref AABB worldHitbox)
    {
        foreach (Entity solidEnt in SolidsQuery.Entities)
        {
            var solidData = solidEnt.Data;
            if (!solidData.Has<HitboxComponent>() || !solidData.Has<Transform2D>()) continue;
            ref var hitbox = ref solidData.Get<HitboxComponent>();
            if (!hitbox.Collidable) continue;
            var solidWorldHitbox = GetWorldHitbox(ref hitbox, ref solidData.Get<Transform2D>());
            if (Collide.QuickCheckAABBToAABB(ref worldHitbox, ref solidWorldHitbox)) return true;
        }
        return false;
    }

    private static void FreeMoveMobile(ref MobileComponent mobile, ref Transform2D mobileTransform, Vector2 movement)
    {
        if (movement.Y != 0)
            mobile.SupportingEntityPid = -1;
        mobileTransform.Position += movement;
        mobile.HighestPoint = Math.Min(mobile.HighestPoint, mobileTransform.Position.Y);
    }

    private ICollision? FindCollision(ref EntityData data, ref Vector2 movement, bool force, ref MobileComponent mobile, ref MobileInfoComponent mobileInfo, ref Vector2 currentFrameMovement, ref AABB quickCheckHitbox, ref float minCollisionTime)
    {
        ICollision? collision = null;
        bool isBounceable = data.Has<BounceableComponent>();
        bool isMortal = data.Has<MortalComponent>();
        bool isLethal = data.Has<LethalComponent>();
        ref var hitbox = ref data.Get<HitboxComponent>();
        ref var transform = ref data.Get<Transform2D>();
        AABB worldMobileHitbox = GetWorldHitbox(ref hitbox, ref transform);

        FindCollisionWithSupport(ref data, currentFrameMovement, ref quickCheckHitbox, ref minCollisionTime, ref collision, ref worldMobileHitbox);
        FindCollisionWithBouncy(ref data, currentFrameMovement, ref quickCheckHitbox, ref minCollisionTime, ref collision, isBounceable, ref worldMobileHitbox);
        FindCollisionWithEdge(movement, force, ref mobile, ref mobileInfo, currentFrameMovement, ref minCollisionTime, ref collision, worldMobileHitbox);
        FindCollisionWithLethal(ref data, currentFrameMovement, ref quickCheckHitbox, ref minCollisionTime, ref collision, ref worldMobileHitbox, isMortal);

        return collision;
    }

    private void FindCollisionWithLethal(ref EntityData data, Vector2 currentFrameMovement, ref AABB quickCheckHitbox, ref float minCollisionTime, ref ICollision? collision, ref AABB worldMobileHitbox, bool isMortal)
    {
        if (!isMortal) return;
        ref var mortal = ref data.Get<MortalComponent>();
        if (!mortal.DiesToLethal) return;

        foreach (Entity lethalEnt in LethalsQuery.Entities)
        {
            if (lethalEnt.Id == data.Id) continue;
            var lethalData = lethalEnt.Data;
            if (!lethalData.Has<HitboxComponent>() || !lethalData.Has<Transform2D>()) continue;
            ref var lethalHitbox = ref lethalData.Get<HitboxComponent>();
            if (!lethalHitbox.Collidable) continue;

            ref var lethalTransform = ref lethalData.Get<Transform2D>();
            ref var lethalC = ref lethalData.Get<LethalComponent>();
            var lethalWorldHitbox = GetWorldHitbox(ref lethalHitbox, ref lethalTransform);
            if (Collide.QuickCheckAABBToAABB(ref worldMobileHitbox, ref lethalWorldHitbox))
            {
                minCollisionTime = 0;
                if (collision is not LethalCollision lethalCollision) lethalCollision = new();
                collision = lethalCollision;
                continue;
            }

            float collisionTime = CheckSweepCollision(ref worldMobileHitbox, ref quickCheckHitbox, currentFrameMovement, minCollisionTime, lethalEnt, out Vector2 normal);
            if (collisionTime != 1)
            {
                minCollisionTime = collisionTime;
                if (collision is not LethalCollision lethalCollision) lethalCollision = new();
                collision = lethalCollision;
            }
        }
    }

    private void FindCollisionWithEdge(Vector2 movement, bool force, ref MobileComponent mobile, ref MobileInfoComponent mobileInfo, Vector2 currentFrameMovement, ref float minCollisionTime, ref ICollision? collision, AABB worldMobileHitbox)
    {
        if (!force && mobileInfo.FlipOnEdge && currentFrameMovement.X != 0 && mobile.SupportingEntityPid >= 0)
        {
            var supportingEntity = Store.GetEntityByPid(mobile.SupportingEntityPid);
            var supportingData = supportingEntity.Data;
            if (supportingData.Has<HitboxComponent>() && supportingData.Has<Transform2D>())
            {
                ref var supportingHitbox = ref supportingData.Get<HitboxComponent>();
                ref var supportingTransform = ref supportingData.Get<Transform2D>().Position;
                float collisionTime;
                if (movement.X > 0)
                {
                    float startRight = worldMobileHitbox.Right;
                    float platformRight = supportingHitbox.Value.Right + supportingTransform.X;
                    float endRight = startRight + currentFrameMovement.X;
                    collisionTime = (platformRight - startRight) / (endRight - startRight);
                }
                else
                {
                    float startLeft = worldMobileHitbox.Left;
                    float platformLeft = supportingHitbox.Value.Left + supportingTransform.X;
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
    }

    private void FindCollisionWithBouncy(ref EntityData data, Vector2 currentFrameMovement, ref AABB quickCheckHitbox, ref float minCollisionTime, ref ICollision? collision, bool isBounceable, ref AABB worldMobileHitbox)
    {
        if (isBounceable)
        {
            foreach (Entity bouncyEnt in BouncyQuery.Entities)
            {
                if (bouncyEnt.Id == data.Id) continue;
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
    }

    private void FindCollisionWithSupport(ref EntityData data, Vector2 currentFrameMovement, ref AABB quickCheckHitbox, ref float minCollisionTime, ref ICollision? collision, ref AABB worldMobileHitbox)
    {
        foreach (Entity supportEnt in SupportsQuery.Entities)
        {
            if (supportEnt.Id == data.Id) continue;
            var supportData = supportEnt.Data;
            ref var supportC = ref supportData.Get<SupportComponent>();

            float collisionTime = CheckSweepCollision(ref worldMobileHitbox, ref quickCheckHitbox, currentFrameMovement, minCollisionTime, supportEnt, out Vector2 normal);
            if (collisionTime != 1)
            {
                bool supportIsSolid = supportData.Tags.Has<SolidTag>();
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

    public static void ApplyBounce(ref float value, float bounce)
    {
        if (Math.Abs(value) < 0.1f)
            value = 0;
        else
            value *= -bounce;
    }

    public static void ApplyBounce(ref Vector2 vector, Vector2 collisionDirection, Vector2 bounce)
    {
        if (collisionDirection.X != 0)
            ApplyBounce(ref vector.X, bounce.X);
        if (collisionDirection.Y != 0)
            ApplyBounce(ref vector.Y, bounce.Y);
    }

    private bool IsAlive(ref EntityData data)
    {
        return !data.Has<MortalComponent>() || !data.Get<MortalComponent>().Dead;
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
