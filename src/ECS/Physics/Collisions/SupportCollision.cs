using DerelictDimension.ECS.Physics.Components;
using Monod.ECS.DefaultComponents;
using Monod.MathModule;

namespace DerelictDimension.ECS.Physics.Collisions;

//TODO: check whether class or struct is better for ICollision implementations. For now i chose class because the thing is boxed anyway.
public class SupportCollision : ICollision
{
    public Vector2 Normal;
    public Entity SupportEntity;

    public void Apply(EntityData _, ref Vector2 movement, float timeRemaining, float collisionTime, ref MobileComponent mobile, ref Position2D mobilePos, ref MobileInfoComponent mobileInfo)
    {
        var data = SupportEntity.Data;
        float timeToMove = Math.Max(0.0f, collisionTime - MathM.Epsilon);
        Vector2 currentFrameMovement = movement * timeRemaining;
        ref SupportComponent support = ref data.Get<SupportComponent>();
        mobilePos.Value += currentFrameMovement * timeToMove;
        if (Normal.X == 0 && Normal.Y == -1)
        {
            mobile.SupportingEntityId = SupportEntity.Id;
            mobile.HighestPoint = float.MaxValue;
        }
        else if (currentFrameMovement.Y != 0)
        {
            mobile.SupportingEntityId = -1;
        }

        Vector2 restitution = PhysicsSystem.GetRestitution(ref mobileInfo, ref support);
        PhysicsSystem.ApplyBounce(ref mobile.Velocity, Normal, restitution);
        PhysicsSystem.ApplyBounce(ref movement, Normal, restitution);
    }
}
