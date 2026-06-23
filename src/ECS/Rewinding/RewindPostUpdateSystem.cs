using Friflo.Engine.ECS.Systems;
using Monod.InputModule;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Rewinding;

public class RewindPostUpdateSystem : BaseSystem
{
    public List<List<StoredComponent>> StoredComponents;
    public EntityStore Store;
    public static int LastValidFrame;

    public RewindPostUpdateSystem()
    {
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
            if (Input.KeyPressed(Key.W)) framesToRewind++;
            if (Input.KeyPressed(Key.Q)) framesToRewind--;

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

            List<ComponentRef> forceRecordNextFrame = new();

            foreach (var (componentRef, recorded) in Rewind.RecordedComponents)
            {
                // entity disabled/enabled
                if (componentRef.ComponentType == null)
                {
                    Entity entity = Store.GetEntityById(componentRef.EntityId);
                    bool currentEnabled = !entity.IsNull && entity.Enabled;
                    bool recordedEnabled = recorded.EnableEntity ?? currentEnabled;
                    bool store = recorded.ForceStore;

                    if (currentEnabled != recordedEnabled)
                    {
                        store = true;
                        forceRecordNextFrame.Add(componentRef);
                    }

                    if (store)
                        StoreComponent(componentRef.EntityId, null, recordedEnabled);

                    continue;
                }

                // component value changed
                IComponent? currentValue = componentRef.Get(Store);
                bool storeComp = recorded.ForceStore;
                IComponent recordedComponent = recorded.Component ?? currentValue;

                if (!recordedComponent.Equals(currentValue))
                {
                    storeComp = true;
                    forceRecordNextFrame.Add(componentRef);
                }

                if (storeComp) StoreComponent(componentRef.EntityId, recordedComponent, true);
            }

            Rewind.RecordedComponents.Clear();
            foreach (var componentRef in forceRecordNextFrame)
                Rewind.RecordedComponents[componentRef] = new(null, true, null);

            Rewind.CurrentFrame++;
        }
        base.OnUpdateGroup();
    }

    private void StoreComponent(int entityId, IComponent? component, bool enableEntity = true)
    {
        StoredComponent storedComponent;
        if (component is not null)
            storedComponent = new(entityId, component, false);
        else
            storedComponent = new(entityId, null, enableEntity);

        StoredComponents[Rewind.CurrentFrame].Add(storedComponent);
    }
}
