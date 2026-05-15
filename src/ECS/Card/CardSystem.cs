using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework;
using Monod.ECS.DefaultComponents;
using Monod.Graphics;
using Monod.InputModule;
using Monod.TimeModule;
using System;

namespace DerelictDimension.ECS.Card;

public class CardSystem : QuerySystem<CardComponent, Position2D, Rotation2D>
{
    public static InputActionIndex LeanLeft;
    public static InputActionIndex LeanRight;
    public static readonly float LeanSpeed = 8f;
    public static readonly float CardLeanLimit = 0.5f;

    protected override void OnUpdate()
    {
        Query.ForEachEntity(Update);
    }

    private void Update(ref CardComponent card, ref Position2D pos, ref Rotation2D rotation, Entity entity)
    {
        Vector2 defaultPos = CalcDefaultPos();
        Rectangle window = Renderer.Window.ClientBounds;
        float target = 0;

        bool leanLeft = Input.ActionDown(LeanLeft);
        bool leanRight = Input.ActionDown(LeanRight);
        if (leanLeft && !leanRight)
        {
            target = -1;
        }
        if (leanRight && !leanLeft)
        {
            target = 1;
        }

        if (card.Lean < target)
        {
            card.Lean += Time.DeltaTime * LeanSpeed;
            if (card.Lean >= target) card.Lean = target;
        }
        else
        {
            card.Lean -= Time.DeltaTime * LeanSpeed;
            if (card.Lean <= target) card.Lean = target;
        }

        float lean = (float)Math.Pow(card.Lean, 3) * CardLeanLimit;

        pos.X = defaultPos.X + (lean * window.Width * 0.25f);
        rotation.Angle = lean;
    }

    private Vector2 CalcDefaultPos()
    {
        Rectangle window = Renderer.Window.ClientBounds;
        return new(window.Width / 2, window.Height / 2);
    }
}
