using Monod.ECS.Systems;

namespace DerelictDimension.ECS.Ai.Damage;

public class DamageCleaupSystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        foreach (var entity in ctx.GetQuery(Queries.DamageRelation).Entities)
        {
            foreach (var relation in entity.GetRelations<DamageRelation>())
            {
                entity.RemoveRelation<DamageRelation, Entity>(relation.GetRelationKey());
            }
        }
    }
}
