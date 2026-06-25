using DerelictDimension.ECS.Physics.Components;
using Monod.ECS.DefaultComponents;

namespace DerelictDimension.ECS.Physics.Collisions;

public class LethalCollision : ICollision
{
    public void Apply(EntityData mobileData, ref Vector2 movement, float timeRemaining, float collisionTime, ref MobileComponent mobile, ref Transform2D mobilePos, ref MobileInfoComponent mobileInfo)
    {
        if (!mobileData.Has<MortalComponent>()) return;
        ref var mortal = ref mobileData.Get<MortalComponent>();
        if (mortal.Dead) return;
        mortal.Kill(mobileData.Id, ref mobileData);
    }
}
