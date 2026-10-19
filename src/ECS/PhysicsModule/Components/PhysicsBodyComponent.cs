using Box2D.NET;

namespace DerelictDimension.ECS.PhysicsModule.Components;

public record struct PhysicsBodyComponent : IComponent
{
    public B2BodyType Type;
    public float Density;
    public float Friction;
    public float Restitution;
    public B2BodyId BodyId;

    public PhysicsBodyComponent(
        B2BodyType type = B2BodyType.b2_dynamicBody,
        float density = 1f,
        float friction = 0.3f,
        float restitution = 0f)
    {
        Type = type;
        Density = density;
        Friction = friction;
        Restitution = restitution;
    }

    public void ApplyTo(ref B2ShapeDef shapeDef)
    {
        shapeDef.density = Density;
        shapeDef.material.friction = Friction;
        shapeDef.material.restitution = Restitution;
    }
}
