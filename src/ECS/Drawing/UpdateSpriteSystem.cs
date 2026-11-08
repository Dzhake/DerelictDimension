using Monod.AssetsModule;
using Monod.ECS.Systems;
using Monod.Graphics.ECS.Sprite;

namespace DerelictDimension.ECS.Drawing;

public class UpdateSpriteSystem : SystemBase
{
    private UpdateEach update = new();

    public override void Update(SystemCtx ctx)
    {
        ctx.GetQuery(Queries.Sprite2D).Each(update);
    }

    public struct UpdateEach : IEach<Sprite2D>
    {
        public void Execute(ref Sprite2D sprite)
        {
            if (sprite.Texture is not null && !Assets.ReloadThisFrame && !sprite.InvalidateTexture) return;
            sprite.InvalidateTexture = false;
            sprite.Texture = string.IsNullOrEmpty(sprite.TexturePath) ? null : new(Assets.Get<Texture2D>(sprite.TexturePath));
        }
    }
}
