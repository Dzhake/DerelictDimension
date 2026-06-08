using Friflo.Engine.ECS.Systems;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Rewinding;

public class RewindPostUpdateSystem : BaseSystem
{
    public List<StoredComponent> StoredComponents;
    public EntityStore Store;
    public static int LastValidIndex;
    public static int CurrentIndex = -1;
    public static int RewindSpeed = -1;

    public RewindPostUpdateSystem()
    {
        //need to reset these values on world reset.
        LastValidIndex = 0;
        CurrentIndex = -1;
        RewindSpeed = -1;
    }

    protected override void OnAddStore(EntityStore store)
    {
        StoredComponents = new();
        Store = store;
        base.OnAddStore(store);
    }

    public static float GetSaturationChange()
    {
        return Math.Sign(RewindSpeed) * 0.8f;
    }

    protected override void OnUpdateGroup()
    {
        if (Rewind.Active)
        {
            int framesToRewind = RewindSpeed;
            int framesSign = Math.Sign(framesToRewind);
            while (framesToRewind != 0 && (CurrentIndex > 0 || RewindSpeed > 0) && (CurrentIndex < LastValidIndex || RewindSpeed <= 0))
            {
                CurrentIndex += framesSign;
                StoredComponent stored = StoredComponents[CurrentIndex];
                if (stored.EntityId == -1)
                {
                    framesToRewind -= framesSign;
                    continue;
                }
                stored.Set(Store);
            }
        }
        else
        {
            foreach (var (componentRef, component) in Rewind.StoredComponents)
            {
                if (component?.Equals(componentRef.Get(Store)) == true) continue;
                StoreComponent(componentRef.EntityId, component);
            }

            StoreComponent(-1, null);
            Rewind.StoredComponents.Clear();
        }
        base.OnUpdateGroup();
    }

    private void StoreComponent(int entityId, IComponent? component)
    {
        StoredComponent storedComponent = new(entityId, component);
        if (CurrentIndex >= StoredComponents.Count - 1)
            StoredComponents.Add(storedComponent);
        else
            StoredComponents[CurrentIndex] = storedComponent;
        CurrentIndex++;
    }
}
