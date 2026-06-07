using Friflo.Engine.ECS;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Rewinding;

public static class Rewind
{
    public static Dictionary<ComponentRef, IComponent> StoredComponents = new();
    public static bool Active;

    public static void Keep<T>(Entity entity, ref T component) where T : IComponent
    {
        if (Rewind.Active || component is null) return;
        ComponentRef key = new(entity.Id, component.GetType());
        if (!StoredComponents.ContainsKey(key))
            StoredComponents.Add(key, component);
    }
}
