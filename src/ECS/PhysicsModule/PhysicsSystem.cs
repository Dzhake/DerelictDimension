using Box2D.NET;
using DerelictDimension.ECS.PhysicsModule.Components;
using DerelictDimension.Utils.Extensions;
using Friflo.Engine.ECS.Systems;
using Monod.ECS.DefaultComponents;
using Monod.TimeModule;
using Monod.Utils.Extensions;
using System.Runtime.CompilerServices;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2MathFunction;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Worlds;

namespace DerelictDimension.ECS.PhysicsModule;

public sealed class PhysicsSystem : BaseSystem
{
    public ArchetypeQuery<PhysicsBodyComponent> PhysicsBodyQuery;
    public ArchetypeQuery<PhysicsShapeComponent> PhysicsShapeQuery;
    public ArchetypeQuery<HitboxComponent> HitboxQuery;
    public ArchetypeQuery<Transform2D> TransformQuery;



    /// <summary>
    /// Currently used only temporarly because i'm not sure whether it's safe to store this between frames/b2 step calls/something else.
    /// </summary>
    public static B2World World;
    public CommandBuffer cb;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        PhysicsBodyQuery = store.Query<PhysicsBodyComponent>();
        PhysicsShapeQuery = store.Query<PhysicsShapeComponent>();
        TransformQuery = store.Query<Transform2D>();
        HitboxQuery = store.Query<HitboxComponent>();

        cb = store.GetCommandBuffer();
        cb.ReuseBuffer = true;
    }

    protected override void OnRemoveStore(EntityStore store)
    {
        cb.ReturnBuffer();
        //TODO (perf: mid) probably should clear the world instead of fully destroying it, but i'm not sure how
        b2DestroyWorld(Physics.WorldId);
        base.OnRemoveStore(store);
    }

    protected override void OnUpdateGroup()
    {
        EnsureWorld();

        SynchronizeBox2d();
        cb.Playback();

        b2World_Step(Physics.WorldId, Time.DeltaTime, 4);

        SynchronizeEcs();
        cb.Playback();

        UnsetDirty();
    }


    private void EnsureWorld()
    {
        if (!b2World_IsValid(Physics.WorldId))
        {
            var b2WorldDef = b2DefaultWorldDef();
            b2WorldDef.gravity = new B2Vec2(0, 0);
            Physics.WorldId = b2CreateWorld(b2WorldDef);
        }
    }

    private void SynchronizeBox2d()
    {
        PhysicsBodyQuery.ForEachEntity(SynchronizeBodyToBox2d);
        PhysicsShapeQuery.ForEachEntity(SynchronizeShapeToBox2d);
    }


    private void SynchronizeBodyToBox2d(ref PhysicsBodyComponent physicsBody, Entity entity)
    {
        var data = entity.Data;
        ref var transform = ref data.Ensure<Transform2D>(cb);

        if (!b2Body_IsValid(physicsBody.BodyId))
        {
            physicsBody.BodyId = CreateBody(ref physicsBody, ref entity);
            return;
        }

        bool setAwake = false;
        if (transform.PhysicsDirty)
        {
            b2Body_SetTransform(physicsBody.BodyId, entity.GetWorldPos(out float rotation).ToB2(), b2MakeRot(rotation));
            setAwake = true;
        }

        if (physicsBody.PhysicsDirty)
        {
            b2Body_SetLinearVelocity(physicsBody.BodyId, physicsBody.LinearVelocity.ToB2());
            b2Body_SetAngularVelocity(physicsBody.BodyId, physicsBody.AngularVelocity);
        }

        if (setAwake) b2Body_SetAwake(physicsBody.BodyId, true);
    }

    private static B2BodyId CreateBody(ref PhysicsBodyComponent physicsBody, ref Entity entity)
    {
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = physicsBody.Type;
        bodyDef.position = entity.GetWorldPos(out float rotation).ToB2();
        bodyDef.rotation = b2MakeRot(rotation);
        bodyDef.angularDamping = 4f;
        bodyDef.linearDamping = 3f;

        bodyDef.angularVelocity = physicsBody.AngularVelocity;
        bodyDef.linearVelocity = physicsBody.LinearVelocity.ToB2();
        if (physicsBody.FixedRotation) bodyDef.motionLocks = new B2MotionLocks { angularZ = true };

        return b2CreateBody(Physics.WorldId, in bodyDef); ;
    }

    private void SynchronizeShapeToBox2d(ref PhysicsShapeComponent physicsShape, Entity entity)
    {
        var data = entity.Data;
        ref var hitbox = ref data.Ensure<HitboxComponent>(cb);
        bool validShape = b2Shape_IsValid(physicsShape.ShapeId);
        bool hasTransform = data.Has<Transform2D>();
        ref var transform = ref (hasTransform ? ref data.Get<Transform2D>() : ref Unsafe.NullRef<Transform2D>());

        if (validShape && (!hasTransform || !transform.PhysicsDirty) && !hitbox.PhysicsDirty) return;
        Entity bodyEntity = entity.GetFirstEntityWithComp<PhysicsBodyComponent>();
        if (bodyEntity.IsNull) return;

        var bodyData = bodyEntity.Data;
        ref var physicsBody = ref bodyData.Get<PhysicsBodyComponent>();
        var bodyPos = bodyEntity.GetWorldPos(out float bodyRot);
        var position = entity.GetWorldPos(out float rotation);
        var relativePos = position + hitbox.Hitbox.Center - bodyPos;
        var relativeRot = rotation + hitbox.Hitbox.Angle - bodyRot;

        ref var rect = ref hitbox.Hitbox;
        var polygon = b2MakeOffsetBox(rect.Width / 2, rect.Height / 2, relativePos.ToB2(), b2MakeRot(relativeRot));

        if (validShape)
        {
            b2Shape_SetPolygon(physicsShape.ShapeId, ref polygon);
        }
        else
        {
            var shapeDef = b2DefaultShapeDef();
            physicsShape.ApplyTo(ref shapeDef);
            physicsShape.ShapeId = b2CreatePolygonShape(physicsBody.BodyId, in shapeDef, in polygon);
        }

        b2Body_SetAwake(physicsBody.BodyId, true);
    }

    private void SynchronizeEcs()
    {
        World = b2GetWorldFromId(Physics.WorldId);
        PhysicsBodyQuery.ForEachEntity(SynchronizeBodyToEcs);
    }

    private void SynchronizeBodyToEcs(ref PhysicsBodyComponent physicsBody, Entity entity)
    {
        var data = entity.Data;
        ref var hitbox = ref data.Ensure<HitboxComponent>(cb);
        ref var transform = ref data.Ensure<Transform2D>(cb);

        B2Transform b2transform = b2GetBodyTransform(World, physicsBody.BodyId.index1 - 1);
        transform.Position = b2transform.p.ToXNA() - hitbox.Hitbox.Center - entity.GetParentPos(out float rotation);
        transform.Rotation = b2Rot_GetAngle(b2transform.q) - hitbox.Hitbox.Angle - rotation;

        physicsBody.LinearVelocity = b2Body_GetLinearVelocity(physicsBody.BodyId).ToXNA();
        physicsBody.AngularVelocity = b2Body_GetAngularVelocity(physicsBody.BodyId);


        //i think shapes are not modified by box2d?
    }

    private void UnsetDirty()
    {
        TransformQuery.ForEachEntity(UnsetTransformDirty);
        HitboxQuery.ForEachEntity(UnsetHitboxDirty);
        PhysicsBodyQuery.ForEachEntity(UnsetPhysicsBodyDirty);
    }

    private void UnsetTransformDirty(ref Transform2D transform, Entity entity) => transform.PhysicsDirty = false;
    private void UnsetHitboxDirty(ref HitboxComponent hitbox, Entity entity) => hitbox.PhysicsDirty = false;
    private void UnsetPhysicsBodyDirty(ref PhysicsBodyComponent physicsBody, Entity entity) => physicsBody.PhysicsDirty = false;


}
