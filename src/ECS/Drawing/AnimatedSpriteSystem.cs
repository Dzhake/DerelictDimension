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
        if (animatedSprite.FrameRemainder > animatedSprite.TimeBetweenFrames)
        {
            animatedSprite.FrameNumber = (animatedSprite.FrameNumber + (int)(animatedSprite.FrameRemainder / animatedSprite.TimeBetweenFrames)) % animatedSprite.Frames.Length;
            animatedSprite.FrameRemainder %= animatedSprite.TimeBetweenFrames;
        }
        string texturePath = animatedSprite.BaseTexturePath + animatedSprite.Frames[animatedSprite.FrameNumber] + ".png";
        if (data.Has<Sprite2D>())
        {
            ref var sprite2D = ref data.Get<Sprite2D>();
            sprite2D.TexturePath = texturePath;
            sprite2D.InvalidateTexture = true;
        }
        else
        {
            CommandBuffer.AddComponent(data.Id, new Sprite2D(texturePath));
        }
    }
}
