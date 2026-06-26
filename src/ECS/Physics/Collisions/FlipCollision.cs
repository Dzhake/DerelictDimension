using DerelictDimension.ECS.Physics.Components;
using Monod.ECS.DefaultComponents;
using Monod.MathModule;

namespace DerelictDimension.ECS.Physics.Collisions;

public class FlipCollision : ICollision
{
    public Vector2 Normal;
    public Vector2 Restitution;

    public void Apply(EntityData _, ref Vector2 movement, float timeRemaining, float collisionTime, ref MobileComponent mobile, ref Transform2D mobileTransform, ref MobileInfoComponent mobileInfo)
    {
        float timeToMove = Math.Max(0.0f, collisionTime - MathM.Epsilon);
        Vector2 currentFrameMovement = movement * timeRemaining;
        mobileTransform.Position += currentFrameMovement * timeToMove;
        if (currentFrameMovement.Y != 0)
            mobile.SupportingEntityPid = -1;

        PhysicsSystem.ApplyBounce(ref mobile.Velocity, Normal, Restitution);
        PhysicsSystem.ApplyBounce(ref movement, Normal, Restitution);
        mobileTransform.FlipX = !mobileTransform.FlipX;
    }
}
