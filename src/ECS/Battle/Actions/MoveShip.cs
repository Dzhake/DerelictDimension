using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;
using Monod.MathModule;
using Monod.TimeModule;
using System;

namespace DerelictDimension.ECS.Battle.Actions;

public struct MoveShip : IShipAction
{
    public Vector2 OldPosition;
    public Vector2 PosDiff;
    public readonly Vector2 NewPosition => OldPosition + PosDiff;
    private int turnsLeft;
    private int totalTurns;
    public int TurnsLeft => turnsLeft;
    public float TimeSpent;
    public float TimePerTurn = 0.4f;
    private bool finished;
    public bool Finished => finished;
    public bool Returning;



    public MoveShip(Vector2 posDiff, int turns)
    {
        PosDiff = posDiff;
        totalTurns = turns;
        turnsLeft = turns;
    }

    public void OnNewTurn(ref FighterComponent fighter, EntityData entity, UpdateBattleSystem system)
    {
        if (totalTurns == turnsLeft)
        {
            OldPosition = fighter.Position;
        }
    }

    public bool Update(ref FighterComponent fighter, EntityData entity, UpdateBattleSystem system)
    {
        TimeSpent += Time.DeltaTime;
        if (TimeSpent >= TimePerTurn)
        {
            turnsLeft--;
            fighter.Position = Vector2.Lerp(OldPosition, NewPosition, (totalTurns - turnsLeft) / totalTurns);
            TimeSpent = 0;
            if (turnsLeft == 0)
            {
                fighter.Position = NewPosition;
                finished = true;
            }
            return true;
        }

        fighter.Position = Vector2.Lerp(OldPosition, NewPosition, Lerp.Cubic(TimeSpent / Math.Max(TimePerTurn, 0.01f) * turnsLeft / totalTurns));
        return false;
    }
}
