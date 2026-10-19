using Box2D.NET;
using DerelictDimension.ECS.PhysicsModule.Components;
using DerelictDimension.Utils.Extensions;
using Friflo.Engine.ECS.Systems;
using Monod.ECS.DefaultComponents;
using Monod.TimeModule;
using Monod.Utils.Extensions;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2MathFunction;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Worlds;

namespace DerelictDimension.ECS.PhysicsModule;

public sealed class PhysicsSystem : BaseSystem
{
    public ArchetypeQuery<HitboxComponent> HitboxQuery;
    public ArchetypeQuery<PhysicsBodyComponent> PhysicsBodyQuery;

    /// <summary>
    /// Currently used only temporarly because i'm not sure whether it's safe to store this between frames/b2 step calls/something else.
    /// </summary>
    public static B2World World;
    public CommandBuffer cb;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        HitboxQuery = store.Query<HitboxComponent>();
        PhysicsBodyQuery = store.Query<PhysicsBodyComponent>();
        cb = store.GetCommandBuffer();
        cb.ReuseBuffer = true;
    }

    protected override void OnRemoveStore(EntityStore store)
    {
        cb.ReturnBuffer();
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
    }

    private void SynchronizeBodyToBox2d(ref PhysicsBodyComponent physicsBody, Entity entity)
    {
        var data = entity.Data;
        ref var hitbox = ref data.Ensure<HitboxComponent>(cb);
        ref var transform = ref data.Ensure<Transform2D>(cb);

        //create the body if missing
        if (physicsBody.BodyId.index1 == 0)
        {
            var bodyDef = b2DefaultBodyDef();
            bodyDef.position = entity.GetWorldPos(out float rotation).ToB2();
            bodyDef.rotation = b2MakeRot(hitbox.Rect.Angle + rotation);
            physicsBody.BodyId = b2CreateBody(Physics.WorldId, in bodyDef);

            hitbox.ShapeId = CreateRectShape(ref physicsBody, ref hitbox);
            //new body is guaranteed to be up-to-date
            hitbox.PhysicsDirty = false;
            transform.PhysicsDirty = false;
            return;
        }

        if (transform.PhysicsDirty)
        {
            b2Body_SetTransform(physicsBody.BodyId, entity.GetWorldPos(out float rotation).ToB2(), b2MakeRot(hitbox.Rect.Angle + rotation));
            transform.PhysicsDirty = false;
        }

        if (hitbox.PhysicsDirty)
        {
            b2DestroyShape(hitbox.ShapeId, true);
            hitbox.ShapeId = CreateRectShape(ref physicsBody, ref hitbox);
            hitbox.PhysicsDirty = false;
        }
    }

    private static B2ShapeId CreateRectShape(ref PhysicsBodyComponent physicsBody, ref HitboxComponent hitbox)
    {
        var shapeDef = b2DefaultShapeDef();
        physicsBody.ApplyTo(ref shapeDef);

        ref var rect = ref hitbox.Rect;
        var polygon = b2MakeOffsetBox(rect.Width / 2, rect.Height / 2, rect.Center.ToB2(), b2MakeRot(rect.Angle));
        return b2CreatePolygonShape(physicsBody.BodyId, in shapeDef, in polygon);
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
}
