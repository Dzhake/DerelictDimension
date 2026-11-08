using Monod.AssetsModule;
using Monod.ECS.Systems;
using Monod.Graphics.Animations;
using Monod.Graphics.ECS.Sprite;
using Monod.TimeModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Drawing;

public class AnimatedSpriteSystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        ctx.GetQuery(Queries.AnimatedSpriteComponent).ForEachEntityM(UpdateEntity, ctx);
    }

    private void UpdateEntity(ref AnimatedSpriteComponent animSprite, Entity entity, SystemCtx ctx)
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


        ref var sprite2D = ref data.Ensure<Sprite2D>(ctx.cb, out bool added);
        if (added || frameChanged || animSprite.InvalidateCurrentTexture)
        {
            sprite2D.TexturePath = animSprite.BaseTexturePath + animSprite.Anim.PathSuffix + animSprite.Anim.Frames[animSprite.FrameNumber];
            sprite2D.InvalidateTexture = true;
        }

        animSprite.InvalidateCurrentTexture = false;
    }
}
