using DerelictDimension.ECS.Ai;
using DerelictDimension.ECS.Ai.Damage;
using DerelictDimension.ECS.Ai.Visual;
using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Graphics.ECS;
using Monod.Graphics.ECS.Sprite;

namespace DerelictDimension.ECS;

[MQuery<Transform2D>]
[MQuery<Sprite2D>]
[MQuery<PhysicsShapeComponent>]
[MQuery<PhysicsBodyComponent>]
[MQuery<HitboxComponent>]
[MQuery<PlayerAi>]
[MQuery<BodyComponent>]
[MQuery<AnimatedSpriteComponent>]

[MQuery<DamageRelation>]

[MQuery<LegsTag>]
[MQuery<ParentVelocityDirTag>]
[MQuery<LookAtMouseTag>]
[MQuery<KickLegComponent>]
[MQuery<YSortSelfTag>]
[MQuery<YSortParentTag>]
public static partial class Queries
{
    public static QueryKey<ArchetypeQuery> All;

    public static void RegisterManual(SystemCtx ctx)
    {
        All = ctx.RegisterQuery(ctx.Store.Query());
    }
}