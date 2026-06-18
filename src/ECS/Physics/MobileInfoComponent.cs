namespace DerelictDimension.ECS.Physics;

/// <summary>
/// Component for storing info about <see cref="MobileComponent"/> that doesn't change often.
/// </summary>
public struct MobileInfoComponent : IComponent
{
    public bool AffectedByGravity = true;
    public bool CanBounce;
    public bool FlipOnEdge;
    public Vector2 Restitution = Vector2.One;
    public float FrictionMult = 1f;


    public MobileInfoComponent()
    {
    }
}
