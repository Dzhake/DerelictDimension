namespace DerelictDimension.ECS.PhysicsModule;

public record class ShapeUserData
{
    public int EntityId;

    public ShapeUserData(int entityId)
    {
        EntityId = entityId;
    }
}
