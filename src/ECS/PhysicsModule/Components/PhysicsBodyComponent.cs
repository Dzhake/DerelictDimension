using Box2D.NET;

namespace DerelictDimension.ECS.PhysicsModule.Components;

public record struct PhysicsBodyComponent : IComponent
{
    public B2BodyType Type;
    public B2BodyId BodyId;

    public PhysicsBodyComponent(B2BodyType type = B2BodyType.b2_dynamicBody)
    {
        Type = type;
    }
}
