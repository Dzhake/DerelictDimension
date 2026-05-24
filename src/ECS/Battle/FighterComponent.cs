using DerelictDimension.ECS.Battle.Actions;
using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Battle;

public struct FighterComponent : IComponent
{
    public readonly Vector2 ShadowOffset = new(2, 2);
    public int Health;
    public int MaxHealth;
    public Vector2 Position;
    public int Team;
    public int IndexInTeam;
    public bool PlayerControlled;
    public List<IShipAction> Actions;
    public bool LooksLeft;

    public FighterComponent()
    {
        Actions = new(3);
    }
}
