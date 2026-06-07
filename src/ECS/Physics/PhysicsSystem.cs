using DerelictDimension.ECS.Physics;
using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework;
using MLEM.Maths;
using Monod.ECS.DefaultComponents;
using Monod.InputModule;
using Monod.TimeModule;
using System;

public class PhysicsSystem : BaseSystem
{
    public readonly Vector2 GravityAccel = new(0, 1000);

    public ArchetypeQuery<ActorComponent> ActorsQuery;
    public ArchetypeQuery<SolidComponent> SolidsQuery;
    public float StepThreshold = 10f;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        ActorsQuery = store.Query<ActorComponent>();
        SolidsQuery = store.Query<SolidComponent>();
    }

    protected override void OnUpdateGroup()
    {
        float dt = Time.DeltaTime;

        foreach (Entity actorEnt in ActorsQuery.Entities)
        {
            var actorData = actorEnt.Data;
            if (!actorData.Has<Position2D>()) continue;
            ref var actor = ref actorData.Get<ActorComponent>();
            ref var actorPos = ref actorData.Get<Position2D>();
            bool isTimeless = actorData.Has<TimelessComponent>();

            if (!Rewind.Active || isTimeless)
            {
                if (!isTimeless)
                {
                    Rewind.Keep(actorEnt, ref actor);
                    Rewind.Keep(actorEnt, ref actorPos);
                }

                actor.Velocity += GravityAccel * dt;
                HandlePlayerInput(actorData, ref actor);

                actorPos.X += actor.Velocity.X * dt;
                RectangleF actorHitbox = actor.Hitbox;
                actorHitbox.Location += actorPos;

                foreach (Entity solidEnt in SolidsQuery.Entities)
                {
                    var solidData = solidEnt.Data;
                    if (!solidData.Has<Position2D>()) continue;
                    ref var solid = ref solidData.Get<SolidComponent>();
                    ref var solidPos = ref solidData.Get<Position2D>();

                    RectangleF solidHitbox = solid.Hitbox;
                    solidHitbox.Location += solidPos;

                    if (actorHitbox.Intersects(solidHitbox))
                    {
                        float stepHeight = actorHitbox.Bottom - solidHitbox.Top;

                        if (stepHeight > 0 && stepHeight <= StepThreshold)
                        {
                            RectangleF stepUpHitbox = actorHitbox;
                            stepUpHitbox.Y -= stepHeight;

                            actorPos.Y -= stepHeight;
                            actorHitbox.Location = actorPos + actor.Hitbox.Location;
                            continue;
                        }

                        if (actor.Velocity.X > 0)
                        {
                            actorPos.X = solidHitbox.Left - actorHitbox.Width - actor.Hitbox.X;
                        }
                        else if (actor.Velocity.X < 0)
                        {
                            actorPos.X = solidHitbox.Right - actor.Hitbox.X;
                        }
                        actor.Velocity.X = 0;
                        actorHitbox.Location = actorPos + actor.Hitbox.Location;
                    }
                }

                actorPos.Y += actor.Velocity.Y * dt;
                actorHitbox = actor.Hitbox;
                actorHitbox.Location += actorPos;

                foreach (Entity solidEnt in SolidsQuery.Entities)
                {
                    var solidData = solidEnt.Data;
                    if (!solidData.Has<Position2D>()) continue;
                    ref var solid = ref solidData.Get<SolidComponent>();
                    ref var solidPos = ref solidData.Get<Position2D>();

                    RectangleF solidHitbox = solid.Hitbox;
                    solidHitbox.Location += solidPos;

                    if (actorHitbox.Intersects(solidHitbox))
                    {
                        if (actor.Velocity.Y > 0)
                        {
                            actorPos.Y = solidHitbox.Top - actorHitbox.Height - actor.Hitbox.Y;
                        }
                        else if (actor.Velocity.Y < 0)
                        {
                            actorPos.Y = solidHitbox.Bottom - actor.Hitbox.Y;
                        }
                        actor.Velocity.Y = 0;
                        actorHitbox.Location = actorPos + actor.Hitbox.Location;
                    }
                }

                if (!actor.InAir)
                {
                    actor.Velocity.X *= 0.95f;
                    if (Math.Abs(actor.Velocity.X) < 0.1f) actor.Velocity.X = 0;
                }
            }


            RectangleF raycastHitbox = actor.Hitbox;
            raycastHitbox.Location += actorPos;
            raycastHitbox.Y += raycastHitbox.Height;
            raycastHitbox.Height = 1f;

            float minY = float.MaxValue;
            actor.RidingEntityId = -1;

            foreach (Entity solidEnt in SolidsQuery.Entities)
            {
                var solidData = solidEnt.Data;
                if (!solidData.Has<Position2D>()) continue;
                ref var solidPos = ref solidData.Get<Position2D>();
                if (minY < solidPos.Y) continue;
                ref var solid = ref solidData.Get<SolidComponent>();

                RectangleF solidHitbox = solid.Hitbox;
                solidHitbox.Location += solidPos;

                if (raycastHitbox.Intersects(solidHitbox))
                {
                    minY = solidPos.Y;
                    actor.RidingEntityId = solidEnt.Id;
                }
            }
        }
    }

    private static void HandlePlayerInput(EntityData actorData, ref ActorComponent actor)
    {
        if (!actorData.Has<PlayerControlledComponent>()) return;

        ref var playerControlled = ref actorData.Get<PlayerControlledComponent>();

        bool left = Input.KeyDown(Key.Left);
        bool right = Input.KeyDown(Key.Right);

        ref float xvel = ref actor.Velocity.X;
        float xAccel = actor.InAir ? playerControlled.AirAcceleration : playerControlled.Acceleration;
        float targetX = xvel;
        if (left && right)
            targetX = 0;
        else if (left)
            targetX = -playerControlled.TargetXVel;
        else if (right)
            targetX = playerControlled.TargetXVel;

        if (xvel > targetX)
        {
            xvel -= xAccel * Time.DeltaTime;
            if (xvel < targetX) xvel = targetX;
        }
        else if (xvel < targetX)
        {
            xvel += xAccel * Time.DeltaTime;
            if (xvel > targetX) xvel = targetX;
        }

        if (Input.KeyDown(Key.Space) && !actor.InAir)
        {
            actor.Velocity.Y = -playerControlled.JumpStrength;
        }
    }

}
