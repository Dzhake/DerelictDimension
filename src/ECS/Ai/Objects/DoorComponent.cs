namespace DerelictDimension.ECS.Ai.Objects;

public record struct DoorComponent : IComponent
{
    public string DebrisTexturePath;

    /// <summary>
    /// multiplier on the door's hitbox width size for the kill zone.
    /// </summary>
    public float KillDistance = 1f;
    public int DebrisCount = 4;
    public float DebrisSpeedMin = 2.5f;
    public float DebrisSpeedMax = 5f;
    public float DebrisLateralJitter = 0.4f;
    public float DebrisSpawnOffset = 0.1f;

    public DoorComponent(string debrisTexturePath)
    {
        DebrisTexturePath = debrisTexturePath;
        KillDistance = 3f;
        DebrisCount = 4;
        DebrisSpeedMin = 2.5f;
        DebrisSpeedMax = 5f;
        DebrisLateralJitter = 0.4f;
        DebrisSpawnOffset = 0.1f;
    }
}

public record struct DebrisTag : ITag;
