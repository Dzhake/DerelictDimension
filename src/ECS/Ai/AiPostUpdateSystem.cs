using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;

namespace DerelictDimension.ECS.Ai;

public class AiPostUpdateSystem : BaseSystem
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

    private void UpdatePlayer(ref PlayerAi ai, Entity entity)
    {
        if (!Rewind.Active || entity.HasComponent<TimelessComponent>())
            ai.PostUpdate(entity, Store);
    }
}
