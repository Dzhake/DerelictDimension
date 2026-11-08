using DerelictDimension.ECS.Ai;
using DerelictDimension.ECS.Ai.Damage;
using DerelictDimension.ECS.Ai.Visual;
using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Graphics.ECS.Sprite;

namespace DerelictDimension.ECS;

[GenerateQueryRegistration]
public static partial class Queries
{
    public static QueryKey<ArchetypeQuery<Transform2D>> Transform2D;
    public static QueryKey<ArchetypeQuery<Sprite2D>> Sprite2D;
    public static QueryKey<ArchetypeQuery<PhysicsShapeComponent>> PhysicsShape;
    public static QueryKey<ArchetypeQuery<PhysicsBodyComponent>> PhysicsBody;
    public static QueryKey<ArchetypeQuery<HitboxComponent>> Hitbox;

    [QueryRelation]
    public static QueryKey<ArchetypeQuery<DamageRelation>> DamageRelation;

    [QueryTag<LegsTag>]
    public static QueryKey<ArchetypeQuery> LegsTag;
    [QueryTag<ParentVelocityDirTag>]
    public static QueryKey<ArchetypeQuery> ParentVelocityDirTag;
    [QueryTag<LookAtMouseTag>]
    public static QueryKey<ArchetypeQuery> LookAtMouseTag;
}