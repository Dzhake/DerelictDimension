using Friflo.Engine.ECS.Systems;

namespace DerelictDimension.ECS.Ai;

public class AiPostUpdateSystem : BaseSystem
{
    public ArchetypeQuery<PlayerAi> PlayerControlledQuery;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        PlayerControlledQuery = store.Query<PlayerAi>();
    }

    protected override void OnUpdateGroup()
    {
        base.OnUpdateGroup();
        PlayerControlledQuery.ForEachEntity(UpdatePlayer);
    }

    private void UpdatePlayer(ref PlayerAi ai, Entity entity) => ai.PostUpdate(entity);
}
