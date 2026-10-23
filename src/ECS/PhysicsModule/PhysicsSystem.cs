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

        //SynchronizeEcs();
        cb.Playback();

        UnsetDirty();
    }


    private void EnsureWorld()
    {
        if (!b2World_IsValid(Physics.WorldId))
        {
            var b2WorldDef = b2DefaultWorldDef();
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

        //create the body if missing
        if (physicsBody.BodyId.index1 == 0)
        {
            var bodyDef = b2DefaultBodyDef();
            bodyDef.position = entity.GetWorldPos(out float rotation).ToB2();
            bodyDef.rotation = b2MakeRot(rotation);
            physicsBody.BodyId = b2CreateBody(Physics.WorldId, in bodyDef);
            return;
        }

        if (transform.PhysicsDirty)
            b2Body_SetTransform(physicsBody.BodyId, entity.GetWorldPos(out float rotation).ToB2(), b2MakeRot(rotation));
    }

    private void SynchronizeShapeToBox2d(ref PhysicsShapeComponent physicsShape, Entity entity)
    {
        var data = entity.Data;
        ref var hitbox = ref data.Ensure<HitboxComponent>(cb);
        bool validShape = b2Shape_IsValid(physicsShape.ShapeId);
        bool hasTransform = data.Has<Transform2D>();
        ref var transform = ref (hasTransform ? ref data.Get<Transform2D>() : ref Unsafe.NullRef<Transform2D>());

        if (validShape && (!hasTransform || !transform.PhysicsDirty) && !hitbox.PhysicsDirty) return;

        var bodyEntity = entity;
        while (!bodyEntity.IsNull && !bodyEntity.HasComponent<PhysicsBodyComponent>())
            bodyEntity = bodyEntity.Parent;
        if (bodyEntity.IsNull) return;

        var bodyData = bodyEntity.Data;
        ref var physicsBody = ref bodyData.Get<PhysicsBodyComponent>();
        var bodyPos = bodyEntity.GetWorldPos(out float bodyRot);
        var position = entity.GetWorldPos(out float rotation);
        var relativePos = position + hitbox.Rect.Center - bodyPos;
        var relativeRot = rotation + hitbox.Rect.Angle - bodyRot;

        ref var rect = ref hitbox.Rect;
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
        transform.Position = b2transform.p.ToXNA() - hitbox.Rect.Center - entity.GetParentPos(out float rotation);
        transform.Rotation = b2Rot_GetAngle(b2transform.q) - hitbox.Rect.Angle - rotation;

        //i think shapes are not modified by box2d?
    }

    private void UnsetDirty()
    {
        TransformQuery.ForEachEntity(UnsetTransformDirty);
        HitboxQuery.ForEachEntity(UnsetHitboxDirty);
    }

    private void UnsetTransformDirty(ref Transform2D transform, Entity entity) => transform.PhysicsDirty = false;
    private void UnsetHitboxDirty(ref HitboxComponent hitbox, Entity entity) => hitbox.PhysicsDirty = false;

}
