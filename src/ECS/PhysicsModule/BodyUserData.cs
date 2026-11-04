namespace DerelictDimension.ECS.PhysicsModule;

public record class BodyUserData
{
    public int EntityId;

    public BodyUserData(int entityId)
    {
        EntityId = entityId;
    }
}
