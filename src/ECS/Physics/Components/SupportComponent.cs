using Monod.MathModule;

namespace DerelictDimension.ECS.Physics.Components;

public record struct SupportComponent : IComponent
{
    /// <summary>
    /// Speed multiplier, applied to mobiles which are supported by this entity. Applied once per frame, at the beginning of physics update.
    /// </summary>
    public float Friction;

    /// <summary>
    /// Whether mobiles supported by this entity should become timeless.
    /// </summary>
    public bool MakeTimeless;

    /// <summary>
    /// In which directions this platform should be able to collide.
    /// </summary>
    public Direction4 Normals;

    /// <summary>
    /// Override bounciness applied to mobile at collision. Values below 0 are considered <see langword="null"/>.
    /// </summary>
    public Vector2 OverrideRestitution;

    /// <summary>
    /// Multiplier to X acceleration of mobiles which are supported by this entity.
    /// </summary>
    public float AccelerationMult = 1;

    //specify one argument to turn it into 'constructor with default arguments' instead of 'constructor without arguments'..
    public SupportComponent() : this(Direction4.Up) { }

    /// <summary>
    /// Create a new instance of support component with the specified values.
    /// </summary>
    /// <param name="normals">In which directions this platform should be able to collide.</param>
    /// <param name="friction">Speed multiplier, applied to mobiles which are supported by this entity. Applied once per frame, at the beginning of physics update.</param>
    /// <param name="makeTimeless">Whether mobiles supported by this entity should become timeless.</param>
    /// <param name="overrideRestitution">Override restitution applied to mobile at collision. Values below 0 are considered <see langword="null"/>. (-1,-1) by default.</param>
    /// <param name="accelerationMult">Multiplier to X acceleration of mobiles which are supported by this entity.</param>
    public SupportComponent(Direction4 normals = Direction4.Up, float friction = 0.95f, bool makeTimeless = false, Vector2? overrideRestitution = null, float accelerationMult = 1)
    {
        Friction = friction;
        MakeTimeless = makeTimeless;
        Normals = normals;
        OverrideRestitution = overrideRestitution ?? new(-1, -1);
        AccelerationMult = accelerationMult;
    }
}
