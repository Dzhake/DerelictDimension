using Friflo.Json.Fliox;
using Monod.ECS.Prefabs;

namespace DerelictDimension.ECS.Prefabs;

public record struct CannonInfoComponent : IComponent
{
    public Vector2[]? Points = null;
    public string PrefabPath;
    public float FiringInterval = 1;
    public float ProjectileVelocity = 300;

    [Ignore]
    public PrefabAsset Prefab;

    //limit amount of entities shot by this cannon?
    //sound effect (asset)
    //sound volume scale (float)

    public CannonInfoComponent(string prefabPath, float firingInterval = 1, Vector2[]? points = null)
    {
        PrefabPath = prefabPath;
        FiringInterval = firingInterval;
        Points = points;
    }
}

public record struct CannonComponent : IComponent
{
    public float TimeUntilNextShot;
}
