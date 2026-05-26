using DerelictDimension.ECS.Battle.Animations;
using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;
using MLEM.Maths;
using Monod.ECS.Tweening;

namespace DerelictDimension.ECS.Battle.Actions;

public class MoveShip : IShipAction
{
    public Vector2 PosDiff;
    private int turnsLeft;
    public int TotalTurns;
    public int TurnsSpent => TotalTurns - turnsLeft;
    public int TurnsLeft => turnsLeft;

    public float TimeSpentThisTurn;
    public float TimePerTurn = 0.4f;


    public MoveShip(Vector2 posDiff, int turns)
    {
        PosDiff = posDiff;
        TotalTurns = turns;
        turnsLeft = turns;
    }

    public void OnNewTurn(ref FighterComponent fighter, Entity entity, UpdateBattleSystem system)
    {
        Vector2 newPos = fighter.Position + (PosDiff / TotalTurns);
        system.Animations.Enqueue(new TweenAnimation<FighterComponent, Vector2, Vector2Lerper>(ref fighter, ref fighter.Position, entity, newPos, 0.4f, Easings.InCubic, new Vector2Lerper()));

        turnsLeft--;
    }
}
