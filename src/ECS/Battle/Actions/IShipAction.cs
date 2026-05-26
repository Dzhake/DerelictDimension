using Friflo.Engine.ECS;

namespace DerelictDimension.ECS.Battle.Actions;

public interface IShipAction
{
    public int TurnsLeft { get; }
    public bool Finished => TurnsLeft == 0;

    public void OnNewTurn(ref FighterComponent fighter, Entity entity, UpdateBattleSystem system);
}
