using Friflo.Engine.ECS.Systems;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Rewinding;

public class RewindPostUpdateSystem : BaseSystem
{
    public List<List<StoredComponent>> StoredComponents;
    public EntityStore Store;
    public static int LastValidFrame;

    public RewindPostUpdateSystem()
    {
        //need to reset these values on world reset.
        LastValidFrame = 0;
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
            while (framesToRewind != 0 && (Rewind.CurrentFrame > 0 || framesSign == 1) && (Rewind.CurrentFrame < LastValidFrame || framesSign == -1))
            {
                Rewind.CurrentFrame += framesSign;
                framesToRewind -= framesSign;
                foreach (StoredComponent stored in StoredComponents[Rewind.CurrentFrame])
                    stored.Set(Store);
            }
        }
        else
        {
            if (Rewind.CurrentFrame >= StoredComponents.Count - 1)
            {
                StoredComponents.Add(new());
            }
            else
            {
                StoredComponents[Rewind.CurrentFrame].Clear();
            }

            foreach (var (componentRef, stored) in Rewind.StoredComponents)
            {
                if (stored.component == null)
                {
                    StoreComponent(componentRef.EntityId, null);
                    Rewind.StoredComponents.Remove(componentRef);
                    return;
                }

                IComponent? currentValue = componentRef.Get(Store);
                bool store = stored.forceStore;
                // component/entity changed
                if (stored.component?.Equals(currentValue) != true)
                {
                    store = true;
                    // force store next frame
                    Rewind.StoredComponents[componentRef] = (null, true);
                }
                else
                {
                    Rewind.StoredComponents.Remove(componentRef);
                }

                if (store) StoreComponent(componentRef.EntityId, stored.component);
            }

            Rewind.CurrentFrame++;
        }
        base.OnUpdateGroup();
    }

    private void StoreComponent(int entityId, IComponent? component)
    {
        StoredComponent storedComponent;
        if (component is not null)
            storedComponent = new(entityId, component);
        else
            storedComponent = new(entityId, null);
        StoredComponents[Rewind.CurrentFrame].Add(storedComponent);
    }
}
