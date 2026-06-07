using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;

namespace DerelictDimension.ECS.Physics;

public struct CircleColliderComponent : IComponent
{
    public float Radius;
    public Vector2 Offset;
}
