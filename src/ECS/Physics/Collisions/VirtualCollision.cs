using Monod.ECS.DefaultComponents;
using Monod.MathModule;

namespace DerelictDimension.ECS.Physics.Collisions;

public class VirtualCollision : ICollision
{
    public Vector2 Normal;
    public Vector2 Restitution;

    public void Apply(ref Vector2 movement, float timeRemaining, float collisionTime, ref MobileComponent mobile, ref Position2D mobilePos, ref MobileInfoComponent mobileInfo)
    {
        float timeToMove = Math.Max(0.0f, collisionTime - MathM.Epsilon);
        Vector2 currentFrameMovement = movement * timeRemaining;
        mobilePos.Value += currentFrameMovement * timeToMove;
        if (currentFrameMovement.Y != 0)
            mobile.SupportingEntityId = -1;

        PhysicsSystem.ApplyBounce(ref mobile.Velocity, Normal, Restitution);
        PhysicsSystem.ApplyBounce(ref movement, Normal, Restitution);
    }
}
