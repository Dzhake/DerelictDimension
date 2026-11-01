using Friflo.Engine.ECS.Systems;
using Monod.AssetsModule;
using Monod.Graphics.Animations;
using Monod.Graphics.ECS.Sprite;
using Monod.TimeModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Drawing;

public class AnimatedSpriteSystem : QuerySystem<AnimatedSpriteComponent>
{
    protected override void OnUpdate()
    {
        Query.ForEachEntity(UpdateEntity);
    }

    private void UpdateEntity(ref AnimatedSpriteComponent animSprite, Entity entity)
    {
        var data = entity.Data;

        if (animSprite.Anim is null || Assets.ReloadThisFrame)
        {
            if (!Animations.Dict.TryGetValue(animSprite.AnimId, out animSprite.Anim)) return;
        }

        if (animSprite.Anim.Frames.Length == 0) return;

        if (animSprite.Anim.Frames.Length - 1 == animSprite.FrameNumber && animSprite.Anim.OnEnd == AnimBehaviour.Stop) return;

        animSprite.FrameRemainder += Time.DeltaTime;

        bool frameChanged = animSprite.FrameRemainder > animSprite.EffectiveTimeBetweenFrames;
        if (frameChanged)
        {
            animSprite.FrameNumber = (animSprite.FrameNumber + (int)(animSprite.FrameRemainder / animSprite.EffectiveTimeBetweenFrames)) % animSprite.Anim.Frames.Length;
            animSprite.FrameRemainder %= animSprite.EffectiveTimeBetweenFrames;
        }

        string texturePath = animSprite.BaseTexturePath + animSprite.Anim.PathSuffix + animSprite.Anim.Frames[animSprite.FrameNumber];

        ref var sprite2D = ref data.Ensure<Sprite2D>(CommandBuffer, out bool added);
        if (added || frameChanged || animSprite.InvalidateCurrentTexture)
        {
            sprite2D.TexturePath = texturePath;
            sprite2D.InvalidateTexture = true;
        }

        animSprite.InvalidateCurrentTexture = false;
    }
}
