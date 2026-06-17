using DerelictDimension.ECS.Physics;
using Monod.ECS.DefaultComponents;

public interface ICollision
{
    public void Apply(ref Vector2 movement, float timeRemaining, float collisionTime, ref MobileComponent mobile, ref Position2D mobilePos, ref MobileInfoComponent mobileInfo);
}
