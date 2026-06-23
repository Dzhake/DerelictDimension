using Monod.Shared.Exceptions;

namespace DerelictDimension.ECS.Rewinding;

public record struct StoredComponent(int EntityId, IComponent? Component, bool EnableEntity)
{
    public readonly void Set(EntityStore store)
    {
        if (EntityId == -1) Guard.Exception($"Tried to apply invalid StoredComponent: {this}");
        Entity entity = store.GetEntityById(EntityId);
        var data = entity.Data;
        if (data.Has<TimelessComponent>())
            return;
        if (Component is null)
        {
            entity.Enabled = EnableEntity;
            return;
        }
        EntitySchema schema = EntityStore.GetEntitySchema();
        var componentType = schema.ComponentTypeByType[Component.GetType()];
        EntityUtils.AddEntityComponentValue(entity, componentType, Component);
    }

    public readonly override string ToString() => $"EntityId: {EntityId}, Component: {Component?.ToString() ?? "<null>"}";
}
