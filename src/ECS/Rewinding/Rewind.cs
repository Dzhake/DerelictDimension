using System.Collections.Generic;

namespace DerelictDimension.ECS.Rewinding;

public static class Rewind
{
    public static Dictionary<ComponentRef, RecordedComponent> RecordedComponents = new();
    public static bool Active;
    public static int CurrentFrame = 0;
    public static int RewindSpeed = -1;

    public static float GetSaturationChange()
    {
        return Math.Sign(Rewind.RewindSpeed) * 0.8f;
    }

    public static void Keep(Entity entity, bool wasEnabled)
    {
        if (Active) return;
        ComponentRef key = new(entity.Id, null);

        if (RecordedComponents.TryGetValue(key, out var recorded))
        {
            recorded.EnableEntity = wasEnabled;
            RecordedComponents[key] = recorded;
            return;
        }

        RecordedComponents.Add(key, new(null, false, wasEnabled));
    }

    public static void Keep<T>(Entity entity, ref T component) where T : IComponent
    {
        if (Active) return;
        ComponentRef key = new(entity.Id, typeof(T));

        if (RecordedComponents.TryGetValue(key, out var recorded))
        {
            if (recorded.Component is not null) return;
            recorded.Component = component;
            RecordedComponents[key] = recorded;
        }
        else
        {
            RecordedComponents.Add(key, new(component, false, null));
        }
    }

    public static bool ShouldUpdateEntity(EntityData data)
    {
        return !Active || data.Has<TimelessComponent>();
    }
}
