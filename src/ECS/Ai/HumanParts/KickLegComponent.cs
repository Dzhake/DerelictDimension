namespace DerelictDimension.ECS.Ai;

public record struct KickLegComponent : IComponent
{
    public long LeftLegPid;
    public long RightLegPid;

    public float KickTimeRemaining;
    public float KickTimeStarting = 0.4f;
    public float KickCooldownRemaining;
    public float KickCooldownStarting = 0.3f;

    public readonly bool KickActive => KickTimeRemaining > 0;

    public KickLegComponent(long leftLegPid, long rightLegPid)
    {
        LeftLegPid = leftLegPid;
        RightLegPid = rightLegPid;
    }
}
