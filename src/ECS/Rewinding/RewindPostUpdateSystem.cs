using Friflo.Engine.ECS.Systems;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Rewinding;

public class RewindPostUpdateSystem : BaseSystem
{
    public List<StoredComponent> StoredComponents;
    public EntityStore Store;
    public static int LastValidIndex;

    public RewindPostUpdateSystem()
    {
        //need to reset these values on world reset.
        LastValidIndex = 0;
        Rewind.CurrentIndex = -1;
        Rewind.RewindSpeed = -1;
    }

    protected override void OnAddStore(EntityStore store)
    {
        StoredComponents = new();
        Store = store;
        base.OnAddStore(store);
    }

    protected override void OnUpdateGroup()
    {
        if (Rewind.Active)
        {
            int framesToRewind = Rewind.RewindSpeed;
            int framesSign = Math.Sign(framesToRewind);
            while (framesToRewind != 0 && (Rewind.CurrentIndex > 0 || Rewind.RewindSpeed > 0) && (Rewind.CurrentIndex < LastValidIndex || Rewind.RewindSpeed <= 0))
            {
                Rewind.CurrentIndex += framesSign;
                StoredComponent stored = StoredComponents[Rewind.CurrentIndex];
                if (stored.EntityId == -1)
                {
                    Rewind.CurrentFrame += framesSign;
                    framesToRewind -= framesSign;
                    continue;
                }
                stored.Set(Store);
            }
            //fix CurrentFrame not resetting to zero because there's no "-1" stored, should probably improve this somehow.
            if (Rewind.CurrentIndex == 0) Rewind.CurrentFrame = 0;
        }
        else
        {
            foreach (var (componentRef, component) in Rewind.StoredComponents)
            {
                if (component?.Equals(componentRef.Get(Store)) == true) continue;
                StoreComponent(componentRef.EntityId, component);
            }

            StoreComponent(-1, null);
            Rewind.CurrentFrame++;
            Rewind.StoredComponents.Clear();
        }
        base.OnUpdateGroup();
    }

    private void StoreComponent(int entityId, IComponent? component)
    {
        StoredComponent storedComponent = new(entityId, component);
        if (Rewind.CurrentIndex >= StoredComponents.Count - 1)
            StoredComponents.Add(storedComponent);
        else
            StoredComponents[Rewind.CurrentIndex] = storedComponent;
        Rewind.CurrentIndex++;
    }
}
