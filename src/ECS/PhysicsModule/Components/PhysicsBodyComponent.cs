using Box2D.NET;
using Friflo.Json.Fliox;

namespace DerelictDimension.ECS.PhysicsModule.Components;

public record struct PhysicsBodyComponent : IComponent
{
    public B2BodyType Type;

    /// <summary>
    /// If <see langword="true"/>, body rotation is locked, and cannot be modified by the box2d.
    /// </summary>
    public bool FixedRotation;

    public Vector2 LinearVelocity;
    public float AngularVelocity;
    public bool PhysicsDirty;

    [Ignore]
    public B2BodyId BodyId;

    public PhysicsBodyComponent(B2BodyType type = B2BodyType.b2_dynamicBody, bool fixedRotation = false)
    {
        Type = type;
        FixedRotation = fixedRotation;
    }
}
