using System.Collections.Generic;

namespace DerelictDimension.ECS.Rewinding;

public static class Rewind
{
    public static Dictionary<ComponentRef, (IComponent? component, bool forceStore)> RecordedComponents = new();
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
        if (RecordedComponents.Remove(key)) return;
        RecordedComponents.Add(key, (null, false));
    }

    public static void Keep<T>(Entity entity, ref T component) where T : IComponent
    {
        if (Active) return;
        ComponentRef key = new(entity.Id, typeof(T));
        //if there's already a value at this ref, but component is null, that means that we explicitly saved 'forceStore', so we need to store the component with that 'forceStore' value.
        if (RecordedComponents.TryGetValue(key, out var recorded))
        {
            if (recorded.component is not null) return;
            RecordedComponents[key] = (component, recorded.forceStore);
        }
        else //just store the component
        {
            RecordedComponents.Add(key, (component, false));
        }
    }

    public static bool ShouldUpdateEntity(EntityData data)
    {
        return !Active || data.Has<TimelessComponent>();
    }
}
