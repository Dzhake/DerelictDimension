using Box2D.NET;
using Friflo.Json.Fliox;

namespace DerelictDimension.ECS.PhysicsModule.Components;

public record struct PhysicsShapeComponent : IComponent
{
    public float Density = 1f;
    public float Friction = 0.6f;
    public float Restitution = 0f;
    public bool IsSensor;

    [Ignore]
    public B2ShapeId ShapeId;

    public PhysicsShapeComponent() { }

    public PhysicsShapeComponent(float density = 1f, float friction = 0.3f, float restitution = 0f, bool isSensor = false)
    {
        Density = density;
        Friction = friction;
        Restitution = restitution;
        IsSensor = isSensor;
    }

    public void ApplyTo(ref B2ShapeDef shapeDef)
    {
        shapeDef.density = Density;
        shapeDef.material.friction = Friction;
        shapeDef.material.restitution = Restitution;
        shapeDef.isSensor = IsSensor;
    }
}
