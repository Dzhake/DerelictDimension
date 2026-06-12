using DerelictDimension.ECS.Physics;
using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;
using Monod.ECS.DefaultComponents;
using Monod.InputModule;
using Monod.MathModule;
using Monod.TimeModule;

public class PhysicsSystem : BaseSystem
{
    public readonly Vector2 GravityAccel = new(0, 1000);

    public ArchetypeQuery<MobileComponent> MobilesQuery;
    public ArchetypeQuery<SupportComponent> SupportsQuery;
    public EntityStore Store;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        MobilesQuery = store.Query<MobileComponent>();
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
                    var supportEnt = Store.GetEntityById(mobile.RidingEntityId);
                    //entity/component were removed for some reason
                    if (supportEnt.IsNull || !supportEnt.HasComponent<SupportComponent>()) return;
                    mobile.Velocity.X *= supportEnt.GetComponent<SupportComponent>().FrictionSpeedMultPerFrame;
                    if (Math.Abs(mobile.Velocity.X) < 0.01f) mobile.Velocity.X = 0;
                }

                mobile.Velocity += GravityAccel * dt;
                HandlePlayerInput(mobileData, ref mobile);

                AABB worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobilePos);

                foreach (Entity supportEnt in SupportsQuery.Entities)
                {
                    var supportData = supportEnt.Data;
                    if (!supportData.Has<Position2D>() || !supportData.Has<HitboxComponent>() || !supportData.Has<SolidComponent>()) continue;

                    ref var supportPos = ref supportData.Get<Position2D>();
                    AABB worldSupportHitbox = GetWorldHitbox(ref supportData.Get<HitboxComponent>(), ref supportPos);

                    if (Collisions.CheckAABBToAABB(ref worldMobileHitbox, ref worldSupportHitbox, out Vector2 mtv))
                    {
                        mobilePos.Value += mtv;
                        if (mtv.X != 0) mobile.Velocity.X = 0;
                        if (mtv.Y != 0) mobile.Velocity.Y = 0;
                        worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobilePos);
                    }
                }

                // Sweeping and sliding
                int maxIterations = 3; //
                float timeRemaining = 1.0f;

                for (int i = 0; i < maxIterations && timeRemaining > 0; i++)
                {
                    Vector2 currentFrameVelocity = mobile.Velocity * dt * timeRemaining;

                    float minCollisionTime = 1.0f;
                    Vector2 finalNormal = Vector2.Zero;

                    // find most recent collision
                    foreach (Entity supportEnt in SupportsQuery.Entities)
                    {
                        var supportData = supportEnt.Data;
                        if (!supportData.Has<Position2D>() || !supportData.Has<HitboxComponent>()) continue;

                        ref var supportPos = ref supportData.Get<Position2D>();
                        bool supportIsSolid = supportData.Has<SolidComponent>();
                        AABB worldSupportHitbox = GetWorldHitbox(ref supportData.Get<HitboxComponent>(), ref supportPos);

                        if (Collisions.SweptCheck(ref worldMobileHitbox, ref worldSupportHitbox, ref currentFrameVelocity, out float collisionTime, out Vector2 normal))
                        {
                            // solid or matching one-way
                            bool canCollide = supportIsSolid || supportData.Get<SupportComponent>().Normals.Matches(normal);

                            if (collisionTime < minCollisionTime && canCollide)
                            {
                                minCollisionTime = collisionTime;
                                finalNormal = normal;
                            }
                        }
                    }

                    if (minCollisionTime < 1.0f)
                    {
                        float timeToMove = Math.Max(0.0f, minCollisionTime - MathM.Epsilon);
                        mobilePos.Value += currentFrameVelocity * timeToMove;

                        timeRemaining -= timeRemaining * minCollisionTime;

                        if (finalNormal.X != 0) mobile.Velocity.X = 0;
                        if (finalNormal.Y != 0) mobile.Velocity.Y = 0;

                        worldMobileHitbox = GetWorldHitbox(ref mobileHitbox, ref mobilePos);
                    }
                    else
                    {
                        mobilePos.Value += currentFrameVelocity;
                        timeRemaining = 0;
                    }
                }



                AABB raycastHitbox = GetWorldHitbox(ref mobileHitbox, ref mobilePos);
                raycastHitbox.CenterY++;

                float maxY = float.MaxValue;
                int ridingEntityId = -1;

                foreach (Entity supportEnt in SupportsQuery.Entities)
                {
                    var supportData = supportEnt.Data;
                    var support = supportData.Get<SupportComponent>();
                    if (!support.Normals.HasFlag(Monod.MathModule.Direction4.Up) && !supportData.Has<SolidComponent>()) continue;
                    if (!supportData.Has<Position2D>()) continue;
                    ref var supportPos = ref supportData.Get<Position2D>();
                    if (maxY < supportPos.Y) continue;
                    if (!supportData.Has<HitboxComponent>()) continue;
                    AABB supportHitbox = supportData.Get<HitboxComponent>().Value;
                    supportHitbox.Center += supportPos.Value;

                    if (Collisions.CheckAABBToAABB(ref raycastHitbox, ref supportHitbox, out var mtv))
                    {
                        maxY = supportPos.Y;
                        ridingEntityId = supportData.Id;
                    }
                }

                mobile.RidingEntityId = ridingEntityId;
            }


            if (mobileData.Has<TimelessComponent>() && !mobileData.Has<TemporaryTimeless>()) continue;
            bool shouldBeTempTimeless = false;
            if (mobile.RidingEntityId != -1)
            {
                Entity ridingEntity = Store.GetEntityById(mobile.RidingEntityId);
                var ridingEntityData = ridingEntity.Data;
                ref var ridingEntitySupportC = ref ridingEntityData.Get<SupportComponent>();
                shouldBeTempTimeless = ridingEntitySupportC.MakeTimeless;
            }
            if (shouldBeTempTimeless) MakeTimeless(mobileData, cb);
            else UnmakeTimeless(mobileData, cb);
        }
        cb.Playback();
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
        LerpFloat(ref xvel, targetX, frameAccel);

        if (Input.KeyDown(Key.Space) && !mobile.InAir)
        {
            mobile.Velocity.Y = -playerControlled.JumpStrength;
            mobile.RidingEntityId = -1;
        }
    }

    private static void LerpFloat(ref float from, float to, float amount)
    {
        if (from > to)
        {
            from -= amount;
            if (from < to) from = to;
        }
        else if (from < to)
        {
            from += amount;
            if (from > to) from = to;
        }
    }
}
