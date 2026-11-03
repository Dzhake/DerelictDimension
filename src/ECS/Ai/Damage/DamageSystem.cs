using Box2D.NET;
using DerelictDimension.ECS.Ai.Objects;
using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.Links;
using DerelictDimension.ECS.PhysicsModule;
using DerelictDimension.ECS.PhysicsModule.Components;
using DerelictDimension.ECS.Prefabs;
using Friflo.Engine.ECS.Systems;
using Monod.ECS.DefaultComponents;
using Monod.Graphics.ECS.Sprite;
using Monod.MathModule;
using Monod.Shared.Exceptions;
using Monod.Utils.Extensions;
using MonoGame.Extended;
using System;
using static Box2D.NET.B2Distances;  // b2MakeProxy
using static Box2D.NET.B2Geometries; // b2MakeOffsetBox, b2MakeRot
using static Box2D.NET.B2Shapes;     // b2Shape_GetUserData
using static Box2D.NET.B2Worlds;     // b2World_OverlapShape

namespace DerelictDimension.ECS.Ai.Damage;

public class DamageSystem : BaseSystem
{
    public ArchetypeQuery Query;
    public CommandBuffer cb;

    /// <summary>
    /// Context for the overlap query performed by box2d.
    /// </summary>
    private readonly DoorKillContext _killContext = new();

    protected override void OnAddStore(EntityStore store)
    {
        Query = store.QueryRelation<DamageRelation>();
        cb = store.GetCommandBuffer();
        cb.ReuseBuffer = true;
        _killContext.cb = cb;
        base.OnAddStore(store);
    }

    protected override void OnUpdateGroup()
    {
        foreach (var victimEnt in Query.Entities)
        {
            var victimData = victimEnt.Data;
            if (victimData.Has<BodyComponent>())
            {
                var victimBodyLinks = victimEnt.GetIncomingLinks<HumanBody>();
                if (victimBodyLinks.Count != 1)
                {
                    Log.Warning("A entity with id '{Id}' and 'BodyComponent' but with {LinksAmount} (1 expected) incoming 'HumanBody' link was attacked", victimEnt.Id, victimBodyLinks.Count);
                    continue;
                }
                var victimBodyLink = victimBodyLinks[0];
                var victimBase = victimBodyLink.Entity;
                AttackHuman(in victimBase, in victimEnt);
            }
            else if (victimData.Has<DoorComponent>())
            {
                var damager = victimEnt.GetRelations<DamageRelation>()[0].Damager; // any hit decides the break side
                BreakDoor(in victimEnt, damager.GetWorldPos(out _));
            }
        }

        cb.Playback();
    }

    private void AttackHuman(in Entity victimBase, in Entity victimPart)
    {
        var victimBaseData = victimBase.Data;
        ref var mortal = ref victimBaseData.Ensure<MortalComponent>(cb);
        if (mortal.Dead) return;

        foreach (var relation in victimPart.GetRelations<DamageRelation>())
        {
            var damagerEnt = relation.Damager;
            var damagerData = damagerEnt.Data;
            if (damagerData.Has<KickLegComponent>())
            {
                damagerEnt.GetWorldPos(out float rotation);
                KillHuman(in victimBase, in victimBaseData, ref mortal, MathM.VectorFromRot(rotation));
            }
        }
    }


    private void BreakDoor(in Entity door, Vector2 damagerPos)
    {
        var doorData = door.Data;
        var doorTransform = doorData.Get<Transform2D>();
        var doorRect = doorData.Get<HitboxComponent>().Rect;
        var doorInfo = doorData.Get<DoorComponent>();

        // ---- door plane geometry, straight from the door's own hitbox ----
        float cos = MathF.Cos(doorTransform.Rotation);
        float sin = MathF.Sin(doorTransform.Rotation);
        var center = doorRect.Center;
        var size = doorRect.Size;
        bool normalIsY = size.X >= size.Y; // doors are wide along local X, thin (normal) along local Y
        var normalLocal = normalIsY ? new Vector2(0f, 1f) : new Vector2(1f, 0f);
        var widthLocal = normalIsY ? new Vector2(1f, 0f) : new Vector2(0f, 1f);
        float halfWidth = (normalIsY ? size.X : size.Y) * 0.5f;
        float halfThickness = (normalIsY ? size.Y : size.X) * 0.5f;

        var normal = Rotate(normalLocal, cos, sin);
        var rectCenterWorld = doorTransform.Position + Rotate(center, cos, sin);

        // The door can only leave its frame along its own normal; break away from the damager.
        float breakSign = Vector2.Dot(rectCenterWorld - damagerPos, normal) >= 0f ? 1f : -1f;
        var breakDir = normal * breakSign;
        var widthDir = Rotate(widthLocal, cos, sin);

        var killHalf = size * doorInfo.KillDistance * 0.5f;
        float killHalfThickness = normalIsY ? killHalf.Y : killHalf.X;
        var killCenter = rectCenterWorld + (breakDir * killHalfThickness);

        var killZone = b2MakeOffsetBox(killHalf.X, killHalf.Y, new B2Vec2(killCenter.X, killCenter.Y), B2MathFunction.b2MakeRot(doorTransform.Rotation));
        var proxy = b2MakeProxy(killZone.vertices.AsSpan(), killZone.count, killZone.radius);

        var worldId = Physics.WorldId;

        var filter = new B2QueryFilter(ulong.MaxValue, (ulong)CollisionLayer.Enemy);

        _killContext.BreakDir = breakDir;
        _killContext.Killed = 0;
        b2World_OverlapShape(worldId, ref proxy, filter, OnKillZoneHit, _killContext);

        // spawn debris
        for (int i = 0; i < doorInfo.DebrisCount; i++)
        {
            float spread = ((i + 0.5f) / doorInfo.DebrisCount * 2f) - 1f; // evenly spaced across the leaf
            float lateral = (spread * halfWidth * 0.8f) + Jitter(doorInfo.DebrisLateralJitter);

            var localOffset = widthLocal * lateral;// + (normalLocal * ((halfThickness + doorInfo.DebrisSpawnOffset) * breakSign));
            var pos = doorTransform.Position + Rotate(center + localOffset, cos, sin);

            float speed = doorInfo.DebrisSpeedMin + (Random.Shared.NextSingle() * (doorInfo.DebrisSpeedMax - doorInfo.DebrisSpeedMin));
            var vel = (breakDir * speed) + (widthDir * Jitter(doorInfo.DebrisLateralJitter * 0.5f));

            CodePrefabs.CreateDoorDebris(cb, pos, Random.Shared.NextSingle() * MathF.PI * 2f, vel, doorInfo.DebrisTexturePath);
        }


        ref var hitbox = ref doorData.Ensure<HitboxComponent>(cb);
        hitbox.Collidable = false;
        hitbox.PhysicsDirty = true;
        ref var doorSprite = ref doorData.Ensure<Sprite2D>(cb);
        doorSprite.Visible = false;

        Log.Information("Door {Id} broken, {Killed} human(s) crushed behind it", door.Id, _killContext.Killed);
    }

    // Called by b2 for every shape overlapping the kill zone. Returns true to continue the query.
    private bool OnKillZoneHit(B2ShapeId shapeId, object context)
    {
        var ctx = (DoorKillContext)context;

        int partId = b2Shape_GetUserData(shapeId).GetRef<ShapeUserData>().EntityId;
        var part = TheGame.Store.GetEntityById(partId);

        var links = part.GetIncomingLinks<HumanBody>();
        if (links.Count != 1) return true;
        var humanBase = links[0].Entity;
        var baseData = humanBase.Data;

        ref var mortal = ref baseData.Ensure<MortalComponent>(ctx.cb);
        if (mortal.Dead) return true;

        KillHuman(in humanBase, in baseData, ref mortal, ctx.BreakDir);
        ctx.Killed++;
        return true;
    }

    private static Vector2 Rotate(Vector2 v, float cos, float sin) => new((v.X * cos) - (v.Y * sin), (v.X * sin) + (v.Y * cos));

    private static float Jitter(float amplitude) => ((Random.Shared.NextSingle() * 2f) - 1f) * amplitude;

    private void KillHuman(in Entity victimBase, in EntityData victimBaseData, ref MortalComponent mortal, Vector2 direction)
    {
        if (!victimBaseData.Has<HumanBody>()) Guard.Exception($"Tried to kill a human without body, human id: {victimBase.Id}");
        var body = victimBaseData.Get<HumanBody>().Body;
        foreach (int childId in victimBase.ChildIds)
        {
            if (childId == body.Id) continue;
            cb.DeleteEntity(childId);
        }

        foreach (int childId in body.ChildIds)
        {
            cb.DeleteEntity(childId);
        }

        mortal.Dead = true;
        mortal.Hp = 0;

        ref var victimTransform = ref victimBaseData.Ensure<Transform2D>(cb);
        victimTransform.PhysicsDirty = true;
        victimTransform.Rotation = direction.ToAngle();
        ref var physicsBody = ref victimBaseData.Ensure<PhysicsBodyComponent>(cb);
        physicsBody.LinearVelocity = direction * 5;
        physicsBody.PhysicsDirty = true;
        ref var bodyAnim = ref body.Data.Ensure<AnimatedSpriteComponent>(cb);
        bodyAnim.SetAnim(Anims.HumanDeath);
        ref var bodyShape = ref body.Data.Ensure<PhysicsShapeComponent>(cb);
        bodyShape.CollisionMask &= CollisionLayer.Walls;
        bodyShape.PhysicsDirty = true;
        Log.Information("Killed {Id}", victimBase.Id);
    }

    private sealed class DoorKillContext
    {
        public CommandBuffer cb;
        public Vector2 BreakDir;
        public int Killed;
    }
}