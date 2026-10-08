using Friflo.Engine.ECS.Systems;

namespace DerelictDimension.ECS.Drawing;

public class AnimatedSpriteSystem : QuerySystem<AnimatedSpriteComponent>
{
    protected override void OnUpdate()
    {
        Query.ForEachEntity(UpdateEntity);
    }

    private void UpdateEntity(ref AnimatedSpriteComponent animatedSprite, Entity entity)
    {

    }
}
