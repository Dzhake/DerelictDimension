namespace DerelictDimension.ECS.Physics.Components;

/// <summary>
/// Component for storing info about <see cref="MobileComponent"/> that doesn't change often.
/// </summary>
public struct MobileInfoComponent : IComponent
{
    public bool AffectedByGravity = true;
    public bool FlipOnEdge;
    public Vector2 Restitution = Vector2.One;
    public float FrictionMult = 1f;


    public MobileInfoComponent() : this(true)
    {
    }

    /// <summary>
    /// Create new instnace of <see cref="MobileInfoComponent"/> with the specified members.
    /// </summary>
    /// <param name="affectedByGravity"></param>
    /// <param name="flipOnEdge"></param>
    /// <param name="restitution"><see cref="Vector2.One"/> by default.</param>
    /// <param name="frictionMult"></param>
    public MobileInfoComponent(bool affectedByGravity = true, bool flipOnEdge = false, Vector2? restitution = null, float frictionMult = 1)
    {
        AffectedByGravity = affectedByGravity;
        FlipOnEdge = flipOnEdge;
        Restitution = restitution ?? Vector2.One;
        FrictionMult = frictionMult;
    }
}
