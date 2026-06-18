using DerelictDimension.ECS.Physics.Components;
using Monod.ECS.DefaultComponents;

public interface ICollision
{
    public void Apply(EntityData mobileData, ref Vector2 movement, float timeRemaining, float collisionTime, ref MobileComponent mobile, ref Position2D mobilePos, ref MobileInfoComponent mobileInfo);
}
