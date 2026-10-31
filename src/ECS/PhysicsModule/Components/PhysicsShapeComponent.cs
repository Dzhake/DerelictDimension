using Box2D.NET;
using DerelictDimension.Utils;
using Friflo.Json.Fliox;

namespace DerelictDimension.ECS.PhysicsModule.Components;

public record struct PhysicsShapeComponent : IAutoIndexedComponent<PhysicsShapeComponent, B2ShapeId>
{
    public float Density = 1f;
    public float Friction = 0.6f;
    public float Restitution = 0f;
    public bool IsSensor;

    [Ignore]
    private B2ShapeId shapeId;
    /// <summary>
    /// Get <see cref="B2ShapeId"/> associated with this shape. Use <see cref="IAutoIndexedComponentExt.SetIndexedValue"/> to set this value.
    /// </summary>
    public readonly B2ShapeId ShapeId => shapeId;

    public PhysicsShapeComponent() { }

    public PhysicsShapeComponent(float density = 1f, float friction = 0.3f, float restitution = 0f, bool isSensor = false)
    {
        Density = density;
        Friction = friction;
        Restitution = restitution;
        IsSensor = isSensor;
    }

    public readonly void ApplyTo(ref B2ShapeDef shapeDef)
    {
        shapeDef.density = Density;
        shapeDef.material.friction = Friction;
        shapeDef.material.restitution = Restitution;
        shapeDef.isSensor = IsSensor;
    }

    ///<inheritdoc/>
    public readonly B2ShapeId GetIndexedValue() => shapeId;

    ///<inheritdoc/>
    public void SetIndexedValueDIRECTLY(B2ShapeId value) => shapeId = value;
}
