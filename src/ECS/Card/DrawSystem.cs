using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monod.AssetsModule;
using Monod.ECS.DefaultComponents;
using Monod.Graphics;
using Monod.Graphics.ECS.Sprite;
using System;

namespace DerelictDimension.ECS.Card;

public class DrawSystem : QuerySystem<CardComponent, Position2D, Rotation2D, Sprite2D>
{
    public static Effect? CardEffect;

    protected override void OnUpdate()
    {
        Query.ForEachEntity(Update);
    }

    private void Update(ref CardComponent card, ref Position2D pos, ref Rotation2D rotation, ref Sprite2D sprite, Entity entity)
    {
        if (CardEffect is null || Assets.ReloadThisFrame)
            CardEffect = Assets.Get<Effect>("Effects/Card.mgfx");
        if (sprite.Texture is null) return;

        Renderer.Begin(samplerState: SamplerState.PointClamp, effect: CardEffect);

        var data = entity.Data;

        Vector2 scale;
        if (data.TryGet<Scale2D>(out var scale2D))
            scale = scale2D.Value;
        else
            scale = Vector2.One;

        float depth;
        if (data.TryGet<RenderDepth>(out var renderDepth))
            depth = renderDepth.Depth;
        else
            depth = 0;

        Vector2 origin = new(sprite.Texture.Width / 2f, sprite.Texture.Height / 2f);
        CardEffect.Parameters["Lean"].SetValue(Math.Abs(UpdateCardSystem.GetActualLean(card.Lean)));
        Renderer.DrawTexture(sprite.Texture, pos.Value, null, sprite.color, rotation.Angle, origin, scale, SpriteEffects.None, depth);

        Renderer.End();
    }
}
