using Friflo.Engine.ECS.Systems;

namespace DerelictDimension.ECS.Ai;

public class AiPreUpdateSystem : BaseSystem
{
    public ArchetypeQuery<PlayerAi> PlayerControlledQuery;

    protected override void OnAddStore(EntityStore store)
    {
        PlayerControlledQuery = store.Query<PlayerAi>();
        base.OnAddStore(store);
    }

    protected override void OnUpdateGroup()
    {
        PlayerControlledQuery.ForEachEntity(UpdatePlayer);
        base.OnUpdateGroup();
    }

    private void UpdatePlayer(ref PlayerAi ai, Entity entity) => ai.PreUpdate(entity);
}
