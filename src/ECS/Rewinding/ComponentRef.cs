using System;

namespace DerelictDimension.ECS.Rewinding;

public record struct ComponentRef
{
    public int EntityId;
    public Type ComponentType;

    public ComponentRef(int entityId, Type componentType)
    {
        EntityId = entityId;
        ComponentType = componentType;
    }
}
