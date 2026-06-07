using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Rewinding;

public class RewindPostUpdateSystem : BaseSystem
{
    public List<StoredComponent> StoredComponents;
    public EntityStore Store;
    public static int CurrentIndex = -1;

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
            while (CurrentIndex > 0)
            {
                CurrentIndex--;
                StoredComponent stored = StoredComponents[CurrentIndex];
                if (stored.EntityId == -1) break;
                stored.Set(Store);
            }
        }
        else
        {
            foreach (var (componentRef, component) in Rewind.StoredComponents)
            {
                if (componentRef.Get(Store).Equals(component)) continue;
                StoredComponent storedComponent = new(componentRef.EntityId, component);

                StoreComponent(storedComponent);
            }

            StoreComponent(new(-1, null));
            Rewind.StoredComponents.Clear();
        }
        base.OnUpdateGroup();
    }

    private void StoreComponent(StoredComponent storedComponent)
    {
        if (CurrentIndex >= StoredComponents.Count - 1)
            StoredComponents.Add(storedComponent);
        else
            StoredComponents[CurrentIndex] = storedComponent;
        CurrentIndex++;
    }
}
