using Friflo.Engine.ECS;
using Monod.Shared.Exceptions;
using System;

namespace DerelictDimension.ECS.Rewinding;

public record struct ComponentRef
{
    public int EntityId;
    public Type? ComponentType;

    public ComponentRef(int entityId, Type? componentType)
    {
        EntityId = entityId;
        ComponentType = componentType;
    }

    public IComponent Get(EntityStore store)
    {
        if (EntityId == -1 || ComponentType == null) Guard.Exception($"Tried to get component of invalid ComponentRef: {this}");
        Entity entity = store.GetEntityById(EntityId);
        EntitySchema schema = EntityStore.GetEntitySchema();
        var componentType = schema.ComponentTypeByType[ComponentType];
        return EntityUtils.GetEntityComponent(entity, componentType);
    }

    public override string ToString() => $"{{ EntityId: {EntityId}, ComponentType: {(ComponentType != null ? ComponentType : "<null>")} }}";
}
