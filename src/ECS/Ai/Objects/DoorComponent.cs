namespace DerelictDimension.ECS.Ai.Objects;

public record struct DoorComponent : IComponent
{
    public string DebrisTexturePath;

    /// <summary>
    /// Multiplier on the door's hitbox width for the kill zone.
    /// </summary>
    public float KillDistance = 1f;
    public int DebrisCount = 6;
    public float DebrisSpeedMin = 8f;
    public float DebrisSpeedMax = 12f;
    public float DebrisLateralJitter = 0.4f;

    public DoorComponent(string debrisTexturePath)
    {
        DebrisTexturePath = debrisTexturePath;
    }
}

public record struct DebrisTag : ITag;
