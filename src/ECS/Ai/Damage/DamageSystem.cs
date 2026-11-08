using Box2D.NET;
using DerelictDimension.ECS.Ai.Objects;
using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.Links;
using DerelictDimension.ECS.PhysicsModule;
using DerelictDimension.ECS.PhysicsModule.Components;
using DerelictDimension.ECS.Prefabs;
using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Graphics.ECS.Sprite;
using Monod.MathModule;
using Monod.Shared.Exceptions;
using Monod.Utils;
using Monod.Utils.Extensions;
using MonoGame.Extended;
using System;
using static Box2D.NET.B2Distances;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Worlds;

namespace DerelictDimension.ECS.Ai.Damage;

public class DamageSystem : SystemBase
{
    /// <summary>
    /// Context for the overlap query performed by box2d.
    /// </summary>
    private readonly DoorKillContext _killContext = new();


    public override void OnCtxChanged(SystemCtx ctx)
    {
        _killContext.cb = ctx.cb;
        _killContext.Store = ctx.Store;
    }

    public override void Update(SystemCtx ctx)
    {
        ctx.GetQuery(Queries.DamageRelation).ForEachEntityM(UpdateEnt, ctx);

        ctx.cb.Playback();
    }

    private void UpdateEnt(ref DamageRelation relation, Entity victimEnt, SystemCtx ctx)
    {
        var victimData = victimEnt.Data;
        if (victimData.Has<BodyComponent>())
        {
            var victimBodyLinks = victimEnt.GetIncomingLinks<HumanBody>();
            if (victimBodyLinks.Count != 1)
            {
                Log.Warning("A entity with id '{Id}' and 'BodyComponent' but with {LinksAmount} (1 expected) incoming 'HumanBody' link was attacked", victimEnt.Id, victimBodyLinks.Count);
                return;
            }
            var victimBodyLink = victimBodyLinks[0];
            var victimBase = victimBodyLink.Entity;
            AttackHuman(in victimBase, in victimEnt, ctx);
        }
        else if (victimData.Has<DoorComponent>())
        {
            var damager = relation.Damager; // any hit decides the break side
            BreakDoor(in victimEnt, damager.GetWorldPos(out _), ctx);
        }
    }

    private void AttackHuman(in Entity victimBase, in Entity victimPart, SystemCtx ctx)
    {
        var victimBaseData = victimBase.Data;
        ref var mortal = ref victimBaseData.Ensure<MortalComponent>(ctx.cb);
        if (mortal.Dead) return;

        foreach (var relation in victimPart.GetRelations<DamageRelation>())
        {
            var damagerEnt = relation.Damager;
            var damagerData = damagerEnt.Data;
            if (damagerData.Has<KickLegComponent>())
            {
                damagerEnt.GetWorldPos(out float rotation);
                KillHuman(in victimBase, in victimBaseData, ref mortal, MathM.VectorFromRot(rotation), ctx.cb);
            }
        }
    }


    private void BreakDoor(in Entity door, Vector2 damagerPos, SystemCtx ctx)
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

        int debrisCount = doorInfo.DebrisCount;
        const float spreadAngle = 120f * (MathF.PI / 180f);

        var spawnPosition = rectCenterWorld;
        float baseAngle = breakDir.ToAngle();

        for (int i = 0; i < debrisCount; i++)
        {
            // Evenly distribute across the forward 120-degree arc.
            float t = (float)i / (debrisCount - 1);
            float angle = baseAngle + ((t - 0.5f) * spreadAngle);

            var direction = MathM.VectorFromRot(angle);

            float speed = doorInfo.DebrisSpeedMin +
                (Randoms.Visual.NextSingle() *
                (doorInfo.DebrisSpeedMax - doorInfo.DebrisSpeedMin));

            CodePrefabs.CreateDoorDebris(ctx.cb, spawnPosition, Randoms.Visual.NextSingle() * MathF.PI * 2f, direction * speed, Randoms.Visual.NextSingle() * MathF.PI * 16f, doorInfo.DebrisTexturePath + (i % 3));
        }


        ref var hitbox = ref doorData.Ensure<HitboxComponent>(ctx.cb);
        hitbox.Collidable = false;
        hitbox.PhysicsDirty = true;
        ref var doorSprite = ref doorData.Ensure<Sprite2D>(ctx.cb);
        doorSprite.Visible = false;

        Log.Information("Door {Id} broken, {Killed} human(s) crushed behind it", door.Id, _killContext.Killed);
    }

    // Called by b2 for every shape overlapping the kill zone. Returns true to continue the query.
    private bool OnKillZoneHit(B2ShapeId shapeId, object context)
    {
        var doorCtx = (DoorKillContext)context;

        int partId = b2Shape_GetUserData(shapeId).GetRef<ShapeUserData>().EntityId;
        var part = doorCtx.Store.GetEntityById(partId);

        var links = part.GetIncomingLinks<HumanBody>();
        if (links.Count != 1) return true;
        var humanBase = links[0].Entity;
        var baseData = humanBase.Data;

        ref var mortal = ref baseData.Ensure<MortalComponent>(doorCtx.cb);
        if (mortal.Dead) return true;

        KillHuman(in humanBase, in baseData, ref mortal, doorCtx.BreakDir, doorCtx.cb);
        doorCtx.Killed++;
        return true;
    }

    private static Vector2 Rotate(Vector2 v, float cos, float sin) => new((v.X * cos) - (v.Y * sin), (v.X * sin) + (v.Y * cos));

    private void KillHuman(in Entity victimBase, in EntityData victimBaseData, ref MortalComponent mortal, Vector2 direction, CommandBuffer cb)
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
        bodyShape.CollisionMask &= CollisionLayer.Walls | CollisionLayer.Environment;
        bodyShape.PhysicsDirty = true;
        Log.Information("Killed {Id}", victimBase.Id);
    }

    private sealed class DoorKillContext
    {
        public CommandBuffer cb;
        public EntityStore Store;
        public Vector2 BreakDir;
        public int Killed;
    }
}