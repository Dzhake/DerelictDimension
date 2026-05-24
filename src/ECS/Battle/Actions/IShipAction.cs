using Friflo.Engine.ECS;

namespace DerelictDimension.ECS.Battle.Actions;

public interface IShipAction
{
    public int TurnsLeft { get; }
    public bool Finished { get; }

    public bool Update(ref FighterComponent fighter, EntityData entity, UpdateBattleSystem system);

    public void OnNewTurn(ref FighterComponent fighter, EntityData entity, UpdateBattleSystem system);
}
