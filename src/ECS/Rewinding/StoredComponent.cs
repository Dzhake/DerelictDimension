using Friflo.Engine.ECS;
using Monod.Shared.Exceptions;

namespace DerelictDimension.ECS.Rewinding;

public record struct StoredComponent
{
    public int EntityId;
    public IComponent? Component;

    public StoredComponent(int entityId, IComponent? component)
    {
        EntityId = entityId;
        Component = component;
    }

    public void Set(EntityStore store)
    {
        if (EntityId == -1 || Component == null) Guard.Exception($"Tried to apply invalid StoredComponent: {this}");
        Entity entity = store.GetEntityById(EntityId);
        EntitySchema schema = EntityStore.GetEntitySchema();
        var componentType = schema.ComponentTypeByType[Component.GetType()];
        EntityUtils.AddEntityComponentValue(entity, componentType, Component);
    }

    public override string ToString() => $"{{ EntityId: {EntityId}, Component: {(Component != null ? Component : "<null>")} }}";
}
