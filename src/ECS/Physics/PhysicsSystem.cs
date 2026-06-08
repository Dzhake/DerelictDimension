using DerelictDimension.ECS.Physics;
using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;
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

                // --- ПРОХОД ПО ОСИ X ---
                actorPos.X += actor.Velocity.X * dt;

                // Создаем мировой хитбокс AABB на основе локального смещения центра и позиции
                AABB worldActorHitbox = new AABB(
                    actor.Hitbox.CenterX + actorPos.X,
                    actor.Hitbox.CenterY + actorPos.Y,
                    actor.Hitbox.HalfWidth,
                    actor.Hitbox.HalfHeight
                );

                foreach (Entity solidEnt in SolidsQuery.Entities)
                {
                    var solidData = solidEnt.Data;
                    if (!solidData.Has<Position2D>()) continue;
                    ref var solid = ref solidData.Get<SolidComponent>();
                    ref var solidPos = ref solidData.Get<Position2D>();

                    RotatedRectangle solidHitbox = solid.Hitbox;
                    solidHitbox.Center += solidPos;

                    if (Collisions.Intersects(worldActorHitbox, solidHitbox, out Vector2 mtv))
                    {
                        // Проверка ступеньки (с учетом нашего предыдущего фикса крутых стен!)
                        if (mtv.Y < 0 && Math.Abs(mtv.Y) <= StepThreshold && Math.Abs(mtv.Y) > Math.Abs(mtv.X))
                        {
                            actorPos.Y += mtv.Y;
                            worldActorHitbox = new AABB(
                                actor.Hitbox.CenterX + actorPos.X,
                                actor.Hitbox.CenterY + actorPos.Y,
                                actor.Hitbox.HalfWidth,
                                actor.Hitbox.HalfHeight
                            );
                            continue;
                        }

                        // Иначе выталкиваем по горизонтали
                        actorPos.X += mtv.X;
                        actor.Velocity.X = 0;
                        worldActorHitbox = new AABB(
                            actor.Hitbox.CenterX + actorPos.X,
                            actor.Hitbox.CenterY + actorPos.Y,
                            actor.Hitbox.HalfWidth,
                            actor.Hitbox.HalfHeight
                        );
                    }
                }

                // --- ПРОХОД ПО ОСИ Y ---
                actorPos.Y += actor.Velocity.Y * dt;
                worldActorHitbox = new AABB(
                    actor.Hitbox.CenterX + actorPos.X,
                    actor.Hitbox.CenterY + actorPos.Y,
                    actor.Hitbox.HalfWidth,
                    actor.Hitbox.HalfHeight
                );

                foreach (Entity solidEnt in SolidsQuery.Entities)
                {
                    var solidData = solidEnt.Data;
                    if (!solidData.Has<Position2D>()) continue;
                    ref var solid = ref solidData.Get<SolidComponent>();
                    ref var solidPos = ref solidData.Get<Position2D>();

                    RotatedRectangle solidHitbox = solid.Hitbox;
                    solidHitbox.Center += solidPos;

                    if (Collisions.Intersects(worldActorHitbox, solidHitbox, out Vector2 mtv))
                    {
                        // Выталкивание по вертикали
                        actorPos.Y += mtv.Y;
                        actor.Velocity.Y = 0;
                        worldActorHitbox = new AABB(
                            actor.Hitbox.CenterX + actorPos.X,
                            actor.Hitbox.CenterY + actorPos.Y,
                            actor.Hitbox.HalfWidth,
                            actor.Hitbox.HalfHeight
                        );
                    }
                }

                if (!actor.InAir)
                {
                    actor.Velocity.X *= 0.95f;
                    if (Math.Abs(actor.Velocity.X) < 0.1f) actor.Velocity.X = 0;
                }
            }


            AABB raycastHitbox = new(
                actor.Hitbox.CenterX + actorPos.X,
                actor.Hitbox.CenterY + actorPos.Y + actor.Hitbox.HalfHeight + 0.5f,
                actor.Hitbox.HalfWidth - 1f,
                0.5f
            );

            float minY = float.MaxValue;
            actor.RidingEntityId = -1;

            foreach (Entity solidEnt in SolidsQuery.Entities)
            {
                var solidData = solidEnt.Data;
                if (!solidData.Has<Position2D>()) continue;
                ref var solidPos = ref solidData.Get<Position2D>();
                if (minY < solidPos.Y) continue;
                ref var solid = ref solidData.Get<SolidComponent>();

                RotatedRectangle solidHitbox = solid.Hitbox;
                solidHitbox.Center += solidPos;

                if (Collisions.Intersects(raycastHitbox, solidHitbox, out var mtv))
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
