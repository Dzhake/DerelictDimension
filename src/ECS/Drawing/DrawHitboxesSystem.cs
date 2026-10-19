using Box2D.NET;
using DerelictDimension.ECS.PhysicsModule;
using DerelictDimension.Utils.Extensions;
using Friflo.Engine.ECS.Systems;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using System;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Worlds;

namespace DerelictDimension.ECS.Drawing;

public class DrawHitboxesSystem : BaseSystem
{
    public static B2DebugDraw draw;

    protected override void OnUpdateGroup()
    {
        Renderer.ShapeBegin();
        b2World_Draw(PhysicsModule.Physics.WorldId, in draw);
        Renderer.ShapeEnd();
        base.OnUpdateGroup();
    }

    protected override void OnAddStore(EntityStore store)
    {
        draw = b2DefaultDebugDraw();
        draw.DrawPointFcn = DrawPoint;
        draw.DrawLineFcn = DrawLine;
        //todo below: draw.DrawTransformFcn = DrawTransform;
        draw.DrawCircleFcn = DrawCircle;
        draw.DrawSolidCircleFcn = DrawSolidCircle;
        draw.DrawPolygonFcn = DrawPolygon;
        draw.DrawSolidPolygonFcn = DrawSolidPolygon;
        //draw.DrawSolidCapsuleFcn = DrawCapsule;
        draw.DrawStringFcn = DrawString;
        base.OnAddStore(store);
    }


    private void DrawPoint(in B2Vec2 p, float size, B2HexColor color, object context)
    {
        Renderer.shapeBatch.BorderCircle(p.ToXNA() * Physics.PixelsInMeter, size * Physics.PixelsInMeter, color.ToXNA());
    }

    private void DrawLine(in B2Vec2 p1, in B2Vec2 p2, B2HexColor color, object context)
    {
        Renderer.shapeBatch.FillLine(p1.ToXNA() * Physics.PixelsInMeter, p2.ToXNA() * Physics.PixelsInMeter, 1, color.ToXNA());
    }

    private void DrawTransform(in B2Transform transform, object context)
    {
        //TODO (box2d debug - high) not sure
    }

    private void DrawCircle(in B2Vec2 center, float radius, B2HexColor color, object context)
    {
        Renderer.shapeBatch.BorderCircle(center.ToXNA() * Physics.PixelsInMeter, radius * Physics.PixelsInMeter, color.ToXNA());
    }

    private void DrawSolidCircle(in B2Transform transform, float radius, B2HexColor color, object context)
    {
        //what's the point of passing transform, is rot doesn't matter for the circle? weird
        Renderer.shapeBatch.FillCircle(transform.p.ToXNA() * Physics.PixelsInMeter, radius * Physics.PixelsInMeter, color.ToXNA());
    }

    private void DrawPolygon(ReadOnlySpan<B2Vec2> vertices, int vertexCount, B2HexColor color, object context)
    {
        var c = color.ToXNA();
        Renderer.shapeBatch.BeginPath(1, c, c);
        for (int i = 0; i < vertexCount + 1; i++)
            Renderer.shapeBatch.PathTo(vertices[i % vertexCount].ToXNA() * Physics.PixelsInMeter);
        Renderer.shapeBatch.EndPath();
    }

    private void DrawSolidPolygon(in B2Transform transform, ReadOnlySpan<B2Vec2> vertices, int vertexCount, float radius, B2HexColor color, object context)
    {
        var c = color.ToXNA();
        Renderer.shapeBatch.BeginPath(radius, c, c);
        for (int i = 0; i < vertexCount + 1; i++)
        {
            Vector2 point = vertices[i % vertexCount].ToXNA();
            point.Rotate(B2MathFunction.b2Rot_GetAngle(transform.q));
            Renderer.shapeBatch.PathTo((point + transform.p.ToXNA()) * Physics.PixelsInMeter);
        }
        Renderer.shapeBatch.EndPath();
    }

    private void DrawCapsule(in B2Vec2 p1, in B2Vec2 p2, float radius, B2HexColor color, object context)
    {
        //TODO (bo2d debug - high) not sure
    }

    private void DrawString(in B2Vec2 p, string s, B2HexColor color, object context)
    {
        Renderer.shapeBatch.DrawString(GlobalFonts.ShapeMenuFont, s, p.ToXNA() * Physics.PixelsInMeter, 5, color.ToXNA());
    }
}
