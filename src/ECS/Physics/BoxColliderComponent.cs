using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;

namespace DerelictDimension.ECS.Physics;

public struct BoxColliderComponent : IComponent
{
    public float HalfWidth;
    public float HalfHeight;
    public Vector2 Offset;
}
