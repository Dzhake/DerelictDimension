using DerelictDimension.ECS.Physics;
using DerelictDimension.ECS.Physics.Collisions;
using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;
using Monod.ECS.DefaultComponents;
using Monod.InputModule;
using Monod.MathModule;
using Monod.TimeModule;

public class PhysicsSystem : BaseSystem
{
    public readonly Vector2 GravityAccel = new(0, 1000);

    public ArchetypeQuery<MobileComponent, MobileInfoComponent> MobilesQuery;
    public ArchetypeQuery<SupportComponent> SupportsQuery;
    public EntityStore Store;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        MobilesQuery = store.Query<MobileComponent, MobileInfoComponent>();
        SupportsQuery = store.Query<SupportComponent>();
        Store = store;
    }

    protected override void OnUpdateGroup()
    {
        float dt = Time.DeltaTime;

        var cb = Store.GetCommandBuffer();

        foreach (Entity mobileEnt in MobilesQuery.Entities)
        {
            var mobileData = mobileEnt.Data;
            if (!mobileData.Has<Position2D>() || !mobileData.Has<HitboxComponent>()) continue;
            ref var mobile = ref mobileData.Get<MobileComponent>();
            ref var mobileInfo = ref mobileData.Get<MobileInfoComponent>();
            ref var mobilePos = ref mobileData.Get<Position2D>();
            ref var mobileHitbox = ref mobileData.Get<HitboxComponent>();
            bool isTimeless = mobileData.Has<TimelessComponent>();
            bool temporaryTimeless = mobileData.Has<TemporaryTimeless>();

            if (!Rewind.Active || isTimeless)
            {
                Rewind.Keep(mobileEnt, in mobile);
                Rewind.Keep(mobileEnt, in mobilePos);

                // Apply forces
                if (mobile.Grounded)
                {
                    var supportEnt = Store.GetEntityById(mobile.SupportingEntityId);
                    //entity/component were removed for some reason
                    if (supportEnt.IsNull || !supportEnt.HasComponent<SupportComponent>()) return;
                    mobile.Velocity.X *= supportEnt.GetComponent<SupportComponent>().FrictionSpeedMultPerFrame * mobileInfo.FrictionMult;
                    if (Math.Abs(mobile.Velocity.X) < 0.01f) mobile.Velocity.X = 0;
                }

                if (mobileInfo.AffectedByGravity)
                    mobile.Velocity += GravityAccel * dt;
                HandlePlayerInput(mobileData, ref mobile);

                AABB worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobilePos);

                foreach (Entity supportEnt in SupportsQuery.Entities)
                {
                    var supportData = supportEnt.Data;
                    if (!supportData.Has<Position2D>() || !supportData.Has<HitboxComponent>() || !supportData.Has<SolidComponent>()) continue;

                    ref var supportPos = ref supportData.Get<Position2D>();
                    ref SupportComponent supportC = ref supportData.Get<SupportComponent>();
                    AABB worldSupportHitbox = GetWorldHitbox(ref supportData.Get<HitboxComponent>(), ref supportPos);

                    //should probably check this in sweep check instead?
                    /*if (Collide.CheckAABBToAABB(ref worldMobileHitbox, ref worldSupportHitbox, out Vector2 mtv))
                    {
                        mobilePos.Value += mtv;
                        ApplyBounce(ref mobile.Velocity, mtv, GetBounce(ref mobileInfo, ref supportC));
                        worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobilePos);
                    }*/
                }

                MoveMobileEntity(mobile.Velocity * dt, ref mobile, ref mobileInfo, ref mobilePos, ref mobileHitbox, ref worldMobileHitbox);
            }


            if (mobileData.Has<TimelessComponent>() && !mobileData.Has<TemporaryTimeless>()) continue;
            bool shouldBeTempTimeless = false;
            if (mobile.SupportingEntityId != -1)
            {
                Entity ridingEntity = Store.GetEntityById(mobile.SupportingEntityId);
                var ridingEntityData = ridingEntity.Data;
                ref var ridingEntitySupportC = ref ridingEntityData.Get<SupportComponent>();
                shouldBeTempTimeless = ridingEntitySupportC.MakeTimeless;
            }
            if (shouldBeTempTimeless) MakeTimeless(mobileData, cb);
            else UnmakeTimeless(mobileData, cb);
        }
        cb.Playback();
    }

    private void MoveMobileEntity(Vector2 movement, ref MobileComponent mobile, ref MobileInfoComponent mobileInfo, ref Position2D mobilePos, ref HitboxComponent mobileHitbox, ref AABB worldMobileHitbox, bool force = false)
    {
        int maxIterations = 5;
        float timeRemaining = 1.0f;

        for (int i = 0; i < maxIterations && timeRemaining > 0; i++)
        {
            Vector2 currentFrameMovement = movement * timeRemaining;
            AABB quickCheckHitbox = worldMobileHitbox.Union(worldMobileHitbox with { Center = worldMobileHitbox.Center + currentFrameMovement });

            float minCollisionTime = 1.0f;
            ICollision? collision = null;


            // find most recent collision
            foreach (Entity supportEnt in SupportsQuery.Entities)
            {
                var supportData = supportEnt.Data;
                if (!supportData.Has<Position2D>() || !supportData.Has<HitboxComponent>()) continue;

                ref var supportPos = ref supportData.Get<Position2D>();
                bool supportIsSolid = supportData.Has<SolidComponent>();
                AABB worldSupportHitbox = GetWorldHitbox(ref supportData.Get<HitboxComponent>(), ref supportPos);

                if (!Collide.QuickCheckAABBToAABB(ref quickCheckHitbox, ref worldSupportHitbox)) continue;

                ref var supportC = ref supportData.Get<SupportComponent>();
                if (Collide.SweptCheck(ref worldMobileHitbox, ref worldSupportHitbox, ref currentFrameMovement, out float collisionTime, out Vector2 normal) && collisionTime < minCollisionTime && (supportIsSolid || supportC.Normals.Matches(normal)))
                {
                    minCollisionTime = collisionTime;
                    if (collision is not SupportCollision supportCollision) supportCollision = new();

                    supportCollision.Normal = normal;
                    supportCollision.SupportEntity = supportEnt;
                    collision = supportCollision;
                }
            }

            //check flip on edge
            if (!force && mobileInfo.FlipOnEdge && currentFrameMovement.X != 0 && mobile.SupportingEntityId >= 0)
            {
                var supportingEntity = Store.GetEntityById(mobile.SupportingEntityId);
                var supportingData = supportingEntity.Data;
                if (supportingData.Has<HitboxComponent>() && supportingData.Has<Position2D>())
                {
                    ref var supportingHitbox = ref supportingData.Get<HitboxComponent>();
                    ref var supportingPos = ref supportingData.Get<Position2D>().Value;
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
                        if (collision is not VirtualCollision virtualCollision) virtualCollision = new();

                        virtualCollision.Normal = new(-Math.Sign(movement.X), 0);
                        virtualCollision.Bounce = new(1, 0);
                        collision = virtualCollision;
                    }
                }
            }


            //apply collision
            if (collision != null)
            {
                collision.Apply(ref movement, timeRemaining, minCollisionTime, ref mobile, ref mobilePos, ref mobileInfo);
                timeRemaining -= timeRemaining * minCollisionTime;

                mobile.HighestPoint = Math.Min(mobile.HighestPoint, mobilePos.Value.Y);
                worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobilePos);
            }
            else
            {
                if (currentFrameMovement.Y != 0)
                    mobile.SupportingEntityId = -1;
                mobilePos.Value += currentFrameMovement;
                mobile.HighestPoint = Math.Min(mobile.HighestPoint, mobilePos.Value.Y);
                timeRemaining = 0;
            }
        }
    }

    public static void ApplyBounce(ref Vector2 vector, Vector2 collisionDirection, Vector2 bounce)
    {
        if (collisionDirection.X != 0)
            vector.X *= -bounce.X;
        if (collisionDirection.Y != 0)
            vector.Y *= -bounce.Y;
    }

    public static Vector2 GetBounce(ref MobileInfoComponent mobileInfo, ref SupportComponent support)
    {
        Vector2 overrideBounciness = support.OverrideBounciness;
        return new(overrideBounciness.X >= 0 ? overrideBounciness.X : mobileInfo.Bounciness.X, overrideBounciness.Y >= 0 ? overrideBounciness.Y : mobileInfo.Bounciness.Y);
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

    private static AABB GetWorldHitbox(ref HitboxComponent hitbox, ref Position2D mobilePos) => new(hitbox.Value.CenterX + mobilePos.X, hitbox.Value.CenterY + mobilePos.Y, hitbox.Value.HalfWidth, hitbox.Value.HalfHeight);

    private static void HandlePlayerInput(EntityData mobileData, ref MobileComponent mobile)
    {
        if (!mobileData.Has<PlayerControlledComponent>()) return;

        ref var playerControlled = ref mobileData.Get<PlayerControlledComponent>();

        bool left = Input.KeyDown(Key.Left);
        bool right = Input.KeyDown(Key.Right);

        ref float xvel = ref mobile.Velocity.X;
        float xAccel = mobile.InAir ? playerControlled.AirAcceleration : playerControlled.Acceleration;
        float targetX = xvel;
        if (left && right)
            targetX = 0;
        else if (left)
            targetX = -playerControlled.TargetXVel;
        else if (right)
            targetX = playerControlled.TargetXVel;

        float frameAccel = xAccel * Time.DeltaTime;
        MathM.LerpFloat(ref xvel, targetX, frameAccel);

        if (Input.KeyDown(Key.Space) && !mobile.InAir)
        {
            mobile.Velocity.Y -= playerControlled.JumpStrength;
            mobile.SupportingEntityId = -1;
        }
    }
}
