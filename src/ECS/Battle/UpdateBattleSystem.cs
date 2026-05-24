using DerelictDimension.ECS.Battle.Actions;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework;
using Monod;
using Monod.ECS.DefaultComponents;
using Monod.Graphics.ECS.Sprite;
using Monod.InputModule;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Battle;

public class UpdateBattleSystem : BaseSystem
{
    public bool InBattle;

    public int ColumnsCount;
    public int ColumnHeight;
    public List<Entity> FightersByInitiative;

    public bool Executing;
    public int CurrentExecutionTurn;
    public int MaxExecutionTurn = 3;
    public int CurrentFighter;
    public bool CalledExecuteForLastAction;

    public int CurrentlySelectedShip;

    public ArchetypeQuery<FighterComponent> FighterQuery;

    ///<inheritdoc/>
    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        FightersByInitiative = new();
        FighterQuery = store.Query<FighterComponent>();
        Test();
    }

    public void Test()
    {
        InBattle = true;
        var player = MonodGame.Store.CreateEntity(new Position2D(), new FighterComponent() { Health = 2, MaxHealth = 2, PlayerControlled = true, Position = Vector2.Zero, IndexInTeam = 0, Team = 0 }, new Sprite2D("GreenShip.png"));
        var enemy = MonodGame.Store.CreateEntity(new Position2D(200, 160), new FighterComponent() { Health = 2, MaxHealth = 2, PlayerControlled = false, Position = new(1, 1), IndexInTeam = 0, Team = 1, LooksLeft = true }, new Sprite2D("MantisShip.png"));
        FightersByInitiative = [player, enemy];
    }

    public static Vector2 GridPosToWorldPos(Vector2 gridPos)
    {
        Vector2 worldSize = TheGame.GameSize;
        return new(worldSize.X / 3 * (gridPos.X + 1), worldSize.Y / 7 * (gridPos.Y + 1));
    }

    /// <summary>
    /// Update current action, and return whether it's finished/invalid.
    /// </summary>
    /// <returns>Whether action is finished/invalid.</returns>
    public bool UpdateCurrentAction()
    {
        if (CurrentFighter > FightersByInitiative.Count) return true;
        var entity = FightersByInitiative[CurrentFighter];
        var data = entity.Data;
        if (!data.Has<FighterComponent>()) return true;
        ref FighterComponent fighter = ref data.Get<FighterComponent>();
        int i = 0;
        IShipAction action;
        do
        {
            if (fighter.Actions.Count <= i) return true;
            action = fighter.Actions[i];
            i++;
        }
        while (action.Finished || action is null);

        if (!CalledExecuteForLastAction)
        {
            CalledExecuteForLastAction = true;
            action.OnNewTurn(ref fighter, data, this);
        }
        return action.Update(ref fighter, data, this);
    }

    protected override void OnUpdateGroup()
    {
        base.OnUpdateGroup();
        if (!InBattle) return;

        if (!Executing)
        {
            if (Input.KeyPressed(Key.W))
            {
                var fighter = FightersByInitiative[CurrentlySelectedShip].GetComponent<FighterComponent>();
                fighter.Actions.Add(new MoveShip(new Vector2(0, -1), 1));
            }
            if (Input.KeyPressed(Key.S))
            {
                var fighter = FightersByInitiative[CurrentlySelectedShip].GetComponent<FighterComponent>();
                fighter.Actions.Add(new MoveShip(new Vector2(0, 1), 1));
            }
            if (Input.KeyPressed(Key.Space))
            {
                Executing = true;
            }
        }

        while (Executing)
        {
            while (CurrentExecutionTurn < MaxExecutionTurn)
            {
                if (!UpdateCurrentAction()) return;
                CalledExecuteForLastAction = false;
                CurrentFighter++;

                if (CurrentFighter >= FightersByInitiative.Count)
                {
                    CurrentFighter = 0;
                    CurrentExecutionTurn++;
                }
            }

            Executing = false;
            CurrentExecutionTurn = 0;
            FighterQuery.ForEachEntity(ErasePlan);
        }
    }

    private void ErasePlan(ref FighterComponent fighter, Entity entity)
    {
        fighter.Actions.Clear();
    }
}
