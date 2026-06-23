namespace DerelictDimension.ECS.Ai;

/// <summary>
/// Interface to be inherited by components that are supposed to be updated in <see cref="AiPreUpdateSystem"/> and <see cref="AiPostUpdateSystem"/>.
/// </summary>
public interface IAi
{
    /// <summary>
    /// Called in <see cref="AiPreUpdateSystem"/>, before physics update.
    /// </summary>
    /// <param name="entity">Entity with this Component.</param>
    void PreUpdate(Entity entity, EntityStore store, CommandBuffer cb);

    /// <summary>
    /// Called in <see cref="AiPostUpdateSystem"/>, before physics update.
    /// </summary>
    /// <param name="entity">Entity with this Component.</param>
    void PostUpdate(Entity entity, EntityStore store, CommandBuffer cb);
}
