using DerelictDimension.ECS.Physics.Components;
using Friflo.Engine.ECS.Systems;

namespace DerelictDimension.ECS.Ai.Cloud;

public class CloudSystem : QuerySystem<CloudBehaviour>
{
    protected override void OnUpdate()
    {
        Query.ForEachEntity(UpdateCloud);
    }

    private void UpdateCloud(ref CloudBehaviour cloud, Entity cloudEnt)
    {
        var data = cloudEnt.Data;
        if (data.Has<MobileComponent>() && data.Has<MortalComponent>() && data.Get<MortalComponent>().Dead)
        {
            data.Get<MobileComponent>().Velocity = Vector2.Zero;
        }
    }
}
