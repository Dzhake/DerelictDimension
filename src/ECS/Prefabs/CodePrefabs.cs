using Box2D.NET;
using DerelictDimension.ECS.Ai;
using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.ECS.DefaultComponents;

namespace DerelictDimension.ECS.Prefabs;

public static class CodePrefabs
{
    public static Entity CreateHuman(Vector2 position, float rotation, string baseTexturePath, out Entity body, out Entity head, out Entity leftLeg, out Entity rightLeg)
    {
        var store = TheGame.Store;

        var entity = store.CreateEntity(new Transform2D(position, rotation: rotation), new MortalComponent(), new PhysicsBodyComponent(B2BodyType.b2_dynamicBody, fixedRotation: true));

        body = store.CreateEntity(new AnimatedSpriteComponent($"{baseTexturePath}body", [0, 1]), new RenderDepth(0.2f), new HitboxComponent(0, 0, 0.82f, 0.27f), new PhysicsShapeComponent());
        entity.AddChild(body);

        head = store.CreateEntity(new HitboxComponent(0, 0, 0.22f, 0.37f), new AnimatedSpriteComponent($"{baseTexturePath}head", [0, 1]), new RenderDepth(0.1f), new PhysicsShapeComponent());
        body.AddChild(head);


        leftLeg = store.CreateEntity(new AnimatedSpriteComponent($"{baseTexturePath}left_leg", [0, 1, 2, 1, 0, 3, 4, 3], 0.1f), new RenderDepth(0.3f), Tags.Get<ParentVelocityDirTag, LegsTag>());
        entity.AddChild(leftLeg);

        rightLeg = store.CreateEntity(new AnimatedSpriteComponent($"{baseTexturePath}right_leg", [0, 1, 2, 1, 0, 3, 4, 3], 0.1f), new RenderDepth(0.3f), Tags.Get<ParentVelocityDirTag, LegsTag>());
        entity.AddChild(rightLeg);

        return entity;
    }
}
