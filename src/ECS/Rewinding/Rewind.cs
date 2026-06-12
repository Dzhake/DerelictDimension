using System.Collections.Generic;

namespace DerelictDimension.ECS.Rewinding;

public static class Rewind
{
    public static Dictionary<ComponentRef, IComponent?> StoredComponents = new();
    public static bool Active;
    public static int CurrentIndex = -1;
    public static int CurrentFrame = 0;
    public static int RewindSpeed = -1;

    public static float GetSaturationChange()
    {
        return Math.Sign(Rewind.RewindSpeed) * 0.8f;
    }

    public static void Keep(Entity entity)
    {
        if (Active) return;
        ComponentRef key = new(entity.Id, null);
        if (StoredComponents.Remove(key)) return;
        StoredComponents.Add(key, null);
    }

    public static void Keep<T>(Entity entity, in T component) where T : IComponent
    {
        if (Active) return;
        ComponentRef key = new(entity.Id, typeof(T));
        if (!StoredComponents.ContainsKey(key))
            StoredComponents.Add(key, component);
    }
}
