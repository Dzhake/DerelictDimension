using DerelictDimension.ECS.Physics.Components;
using Monod.ECS.DefaultComponents;
using Monod.MathModule;

namespace DerelictDimension.ECS.Physics.Collisions;

//TODO (very low priority): check whether class or struct is better for ICollision implementations. For now i chose class because the thing is boxed anyway.
public class SupportCollision : ICollision
{
    public Vector2 Normal;
    public Entity SupportEntity;

    public void Apply(EntityData _, ref Vector2 movement, float timeRemaining, float collisionTime, ref MobileComponent mobile, ref Transform2D mobilePos, ref MobileInfoComponent mobileInfo)
    {
        var data = SupportEntity.Data;
        float timeToMove = Math.Max(0.0f, collisionTime - MathM.Epsilon);
        Vector2 currentFrameMovement = movement * timeRemaining;
        ref SupportComponent support = ref data.Get<SupportComponent>();
        mobilePos.Position += currentFrameMovement * timeToMove;
        if (Normal == MathM.VectorUp)
        {
            mobile.SupportingEntityPid = SupportEntity.Pid;
            mobile.HighestPoint = float.MaxValue;
        }
        else if (currentFrameMovement.Y != 0)
        {
            mobile.SupportingEntityPid = -1;
        }

        if (data.Tags.Has<FragileTag>() && data.Has<MortalComponent>())
        {
            data.Get<MortalComponent>().Kill(data.Id, ref data);
            return;
        }

        Vector2 restitution = PhysicsSystem.GetRestitution(ref mobileInfo, ref support);
        if (Math.Abs(restitution.X) < 0.001f) restitution.X = 0;
        if (Math.Abs(restitution.Y) < 0.001f) restitution.Y = 0;

        if (Normal.X != 0)
        {
            ApplyBounce(ref mobile.Velocity.X, mobileInfo.RestitutionRequiredVelocity.X, mobileInfo.RestitutionMinimumResultingVelocity.X, restitution);
            ApplyBounce(ref movement.X, mobileInfo.RestitutionRequiredVelocity.X, mobileInfo.RestitutionMinimumResultingVelocity.X, restitution);
        }

        if (Normal.Y != 0)
        {
            ApplyBounce(ref mobile.Velocity.Y, mobileInfo.RestitutionRequiredVelocity.Y, mobileInfo.RestitutionMinimumResultingVelocity.Y, restitution);
            ApplyBounce(ref movement.Y, mobileInfo.RestitutionRequiredVelocity.Y, mobileInfo.RestitutionMinimumResultingVelocity.Y, restitution);
        }
    }

    private void ApplyBounce(ref float value, float required, float minimumResulting, Vector2 restitution)
    {
        if (Math.Abs(value) > required)
        {
            PhysicsSystem.ApplyBounce(ref value, restitution.Y);
            if (Math.Abs(value) < minimumResulting) value = 0;
        }
        else
        {
            value = 0;
        }
    }
}
