using Monod.MathModule;

namespace DerelictDimension.ECS.Physics;

public struct SupportComponent : IComponent
{
    public float FrictionSpeedMultPerFrame;
    public bool MakeTimeless;
    public Direction4 Normals;
    public Vector2 OverrideBounciness = new(-1, -1);

    //specify one argument to turn it into 'constructor with default arguments' instead of 'constructor without arguments'..
    public SupportComponent() : this(Direction4.Up) { }

    public SupportComponent(Direction4 normals = Direction4.Up, float friction = 0.95f, bool makeTimeless = false)
    {
        FrictionSpeedMultPerFrame = friction;
        MakeTimeless = makeTimeless;
        Normals = normals;
    }
}
