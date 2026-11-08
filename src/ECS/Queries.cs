using DerelictDimension.ECS.Ai;
using DerelictDimension.ECS.Ai.Damage;
using DerelictDimension.ECS.Ai.Visual;
using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Graphics.ECS.Sprite;

namespace DerelictDimension.ECS;

public static class Queries
{
    public static QueryKey<ArchetypeQuery<Transform2D>> Transform2D;
    public static QueryKey<ArchetypeQuery<Sprite2D>> Sprite2D;
    public static QueryKey<ArchetypeQuery<PhysicsShapeComponent>> PhysicsShape;
    public static QueryKey<ArchetypeQuery<PhysicsBodyComponent>> PhysicsBody;
    public static QueryKey<ArchetypeQuery<HitboxComponent>> Hitbox;

    public static QueryKey<ArchetypeQuery<DamageRelation>> DamageRelation;

    public static QueryKey<ArchetypeQuery> LegsTag;
    public static QueryKey<ArchetypeQuery> ParentVelocityDirTag;
    public static QueryKey<ArchetypeQuery> LookAtMouseTag;



    public static void Register(SystemCtx ctx)
    {
        var store = ctx.Store;
        Transform2D = ctx.RegisterQuery(store.Query<Transform2D>());
        Sprite2D = ctx.RegisterQuery(store.Query<Sprite2D>());
        PhysicsShape = ctx.RegisterQuery(store.Query<PhysicsShapeComponent>());
        PhysicsBody = ctx.RegisterQuery(store.Query<PhysicsBodyComponent>());
        Hitbox = ctx.RegisterQuery(store.Query<HitboxComponent>());

        DamageRelation = ctx.RegisterQuery(store.QueryRelation<DamageRelation>());

        LegsTag = ctx.RegisterQuery(store.Query().AllTags(Tags.Get<LegsTag>()));
        ParentVelocityDirTag = ctx.RegisterQuery(store.Query().AllTags(Tags.Get<ParentVelocityDirTag>()));
        LookAtMouseTag = ctx.RegisterQuery(store.Query().AllTags(Tags.Get<LookAtMouseTag>()));
    }
}
