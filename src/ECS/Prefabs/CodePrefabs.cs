using Box2D.NET;
using DerelictDimension.ECS.Ai;
using DerelictDimension.ECS.Ai.Objects;
using DerelictDimension.ECS.Ai.Visual;
using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.Links;
using DerelictDimension.ECS.PhysicsModule;
using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.ECS.DefaultComponents;
using Monod.Graphics.ECS;
using Monod.Graphics.ECS.Sprite;
using Monod.MathModule;

namespace DerelictDimension.ECS.Prefabs;

public static class CodePrefabs
{
    public static Entity CreateHuman(Vector2 position, float rotation, string baseTexturePath, out Entity body, out Entity head, out Entity leftLeg, out Entity rightLeg, float baseDepth = 100f)
    {
        var store = EcsMain.CurrentStore;

        body = store.CreateEntity(new EntityName("Body"), new AnimatedSpriteComponent(baseTexturePath, Anims.HumanBodyBreathe), new RenderDepth(baseDepth, 10), new HitboxComponent(new Capsule(new(-.25f, 0), new(.25f, 0), 0.1f)), new PhysicsShapeComponent(categories: CollisionLayer.Enemy, mask: CollisionLayer.Walls | CollisionLayer.PlayerAttacks | CollisionLayer.Environment), new BodyComponent(), Tags.Get<YSortParentTag>());

        head = store.CreateEntity(new EntityName("Head"), new AnimatedSpriteComponent(baseTexturePath, Anims.HumanHeadBreathe), new RenderDepth(baseDepth, 0), Tags.Get<YSortParentTag>());

        leftLeg = store.CreateEntity(new EntityName("Left leg"), new AnimatedSpriteComponent(baseTexturePath, Anims.HumanLeftLeg), new RenderDepth(baseDepth, 20), Tags.Get<ParentVelocityDirTag, LegsTag, YSortParentTag>());

        rightLeg = store.CreateEntity(new EntityName("Right leg"), new AnimatedSpriteComponent(baseTexturePath, Anims.HumanRightLeg), new RenderDepth(baseDepth, 21), Tags.Get<ParentVelocityDirTag, LegsTag, YSortParentTag>());


        var entity = store.CreateEntity(new EntityName("Human base"), new Transform2D(position, rotation: rotation), new MortalComponent(), new PhysicsBodyComponent(B2BodyType.b2_dynamicBody, fixedRotation: true), new HumanBody(body), new HumanHead(head), new HumanLeftLeg(leftLeg), new HumanRightLeg(rightLeg), Tags.Get<YSortSelfTag>());


        entity.AddChild(body);
        body.AddChild(head);
        entity.AddChild(leftLeg);
        entity.AddChild(rightLeg);

        return entity;
    }

    public static Entity CreateDoor(Vector2 position, float rotation, Vector2 hitboxSize, string texturePath, in DoorComponent doorComponent)
    {
        return EcsMain.CurrentStore.CreateEntity(new EntityName("Door"), new Transform2D(position, rotation: rotation),
            new HitboxComponent(new OrientedRect(Vector2.Zero, hitboxSize)), new Sprite2D(texturePath),
            new PhysicsBodyComponent(type: B2BodyType.b2_staticBody), new PhysicsShapeComponent(categories: CollisionLayer.Environment, mask: CollisionLayer.All),
            doorComponent);
    }

    // Created through the command buffer so it can be called during query iteration.
    public static void CreateDoorDebris(CommandBuffer cb, Vector2 position, float rotation, Vector2 linearVelocity, float angularVelocity, string texturePath)
    {
        const float debrisHitboxSize = 0.25f;

        int id = cb.CreateEntity();
        cb.AddComponent(id, new EntityName("Door debris"));
        cb.AddComponent(id, new Transform2D(position, rotation: rotation));
        cb.AddComponent(id, new HitboxComponent(new OrientedRect(Vector2.Zero, new Vector2(debrisHitboxSize, debrisHitboxSize))));
        cb.AddComponent(id, new Sprite2D(texturePath));
        cb.AddComponent(id, new PhysicsBodyComponent(B2BodyType.b2_dynamicBody, fixedRotation: false) { LinearVelocity = linearVelocity, AngularVelocity = angularVelocity });
        cb.AddComponent(id, new PhysicsShapeComponent(categories: CollisionLayer.Environment, mask: CollisionLayer.Walls));
        cb.AddTag<DebrisTag>(id);
    }
}
