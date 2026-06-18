using Friflo.Engine.ECS.Systems;

namespace DerelictDimension.ECS.Ai;

public class AiPreUpdateSystem : BaseSystem
{
    public ArchetypeQuery<PlayerAi> PlayerControlledQuery;
    public EntityStore Store;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        PlayerControlledQuery = store.Query<PlayerAi>();
        Store = store;
    }

    protected override void OnUpdateGroup()
    {
        base.OnUpdateGroup();
        PlayerControlledQuery.ForEachEntity(UpdatePlayer);
    }

    private void UpdatePlayer(ref PlayerAi ai, Entity entity) => ai.PreUpdate(entity, Store);
}
