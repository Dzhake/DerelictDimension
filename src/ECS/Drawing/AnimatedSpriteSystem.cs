using Friflo.Engine.ECS.Systems;
using Monod.Graphics.ECS.Sprite;
using Monod.TimeModule;

namespace DerelictDimension.ECS.Drawing;

public class AnimatedSpriteSystem : QuerySystem<AnimatedSpriteComponent>
{
    protected override void OnUpdate()
    {
        Query.ForEachEntity(UpdateEntity);
    }

    private void UpdateEntity(ref AnimatedSpriteComponent animatedSprite, Entity entity)
    {
        var data = entity.Data;

        animatedSprite.FrameRemainder += Time.DeltaTime;

        bool frameChanged = animatedSprite.FrameRemainder > animatedSprite.TimeBetweenFrames;
        if (frameChanged)
        {
            animatedSprite.FrameNumber = (animatedSprite.FrameNumber + (int)(animatedSprite.FrameRemainder / animatedSprite.TimeBetweenFrames)) % animatedSprite.Frames.Length;
            animatedSprite.FrameRemainder %= animatedSprite.TimeBetweenFrames;
        }

        string texturePath = animatedSprite.BaseTexturePath + animatedSprite.Frames[animatedSprite.FrameNumber];
        if (data.Has<Sprite2D>())
        {
            if (frameChanged || animatedSprite.BasePathChanged)
            {
                ref var sprite2D = ref data.Get<Sprite2D>();
                sprite2D.TexturePath = texturePath;
                sprite2D.InvalidateTexture = true;
            }
        }
        else
        {
            CommandBuffer.AddComponent(data.Id, new Sprite2D(texturePath));
        }

        animatedSprite.BasePathChanged = false;
    }
}
