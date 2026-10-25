using Box2D.NET;

namespace DerelictDimension.ECS.PhysicsModule.Components;

public record struct PhysicsShapeComponent : IComponent
{
    public float Density = 1f;
    public float Friction = 0.6f;
    public float Restitution = 0f;
    public B2ShapeId ShapeId;

    public PhysicsShapeComponent() { }

    public PhysicsShapeComponent(float density = 1f,
        float friction = 0.3f,
        float restitution = 0f)
    {
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
