using System.Collections.Generic;

namespace DerelictDimension.ECS.Rewinding;

public static class Rewind
{
    public static Dictionary<ComponentRef, (IComponent? component, bool forceStore)> StoredComponents = new();
    public static bool Active;
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
        // this method is for toggling entity state, so if it was toggled earlier this frame we toggle it back.
        if (StoredComponents.Remove(key)) return;
        StoredComponents.Add(key, (null, false));
    }

    public static void Keep<T>(Entity entity, in T component) where T : IComponent
    {
        if (Active) return;
        ComponentRef key = new(entity.Id, typeof(T));
        //if there's already a value at this ref, but component is null, that means that we explicitly saved 'forceStore', so we need to store the component with that 'forceStore' value.
        if (StoredComponents.TryGetValue(key, out var stored))
        {
            if (stored.component is not null) return;
            StoredComponents[key] = (component, stored.forceStore);
        }
        else //just store the component
        {
            StoredComponents.Add(key, (component, false));
        }
    }

    public static bool ShouldUpdateEntity(EntityData data)
    {
        return !Active || data.Has<TimelessComponent>();
    }
}
