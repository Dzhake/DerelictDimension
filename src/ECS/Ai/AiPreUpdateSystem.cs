using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;

namespace DerelictDimension.ECS.Ai;

public class AiPreUpdateSystem : BaseSystem
{
    public ArchetypeQuery<PlayerAi> PlayerControlledQuery;
    public ArchetypeQuery<MonstarAi> MonstarControlledQuery;
    public EntityStore Store;
    public CommandBuffer cb;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        PlayerControlledQuery = store.Query<PlayerAi>();
        MonstarControlledQuery = store.Query<MonstarAi>();
        Store = store;
        cb = store.GetCommandBuffer();
        cb.ReuseBuffer = true;
    }

    protected override void OnUpdateGroup()
    {
        base.OnUpdateGroup();
        PlayerControlledQuery.ForEachEntity(UpdateAi);
        MonstarControlledQuery.ForEachEntity(UpdateAi);
        cb.Playback();
    }

    public void UpdateAi<T>(ref T ai, Entity entity) where T : IAi
    {
        if (Rewind.ShouldUpdateEntity(entity.Data))
            ai.PreUpdate(entity, Store, cb);
    }
}
