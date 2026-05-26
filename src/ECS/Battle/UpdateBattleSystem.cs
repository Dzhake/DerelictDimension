using DerelictDimension.ECS.Battle.Actions;
using DerelictDimension.ECS.Battle.Animations;
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
    public Queue<IAnimation> Animations;

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
    /// Update current element, and return whether it's finished/invalid.
    /// </summary>
    /// <returns>Whether element is finished/invalid.</returns>
    public void ExecuteNextAction()
    {
        if (CurrentFighter > FightersByInitiative.Count) return;
        var entity = FightersByInitiative[CurrentFighter];
        var data = entity.Data;
        if (!data.Has<FighterComponent>()) return;
        ref FighterComponent fighter = ref data.Get<FighterComponent>();
        ref IShipAction? action = ref fighter.Actions[0];
        while (true)
        {
            if (action == null) return;

            if (action?.Finished != false)
            {
                fighter.Actions.RemoveAt(0);
                action = ref fighter.Actions[0];
            }
            else
            {
                break;
            }

        }

        action.OnNewTurn(ref fighter, entity, this);
    }

    protected override void OnUpdateGroup()
    {
        base.OnUpdateGroup();
        if (!InBattle) return;

        if (!Executing)
        {
            ProcessPlanningInput();
        }

        while (Executing)
        {
            while (CurrentExecutionTurn < MaxExecutionTurn)
            {
                while (Animations.Peek().Update())
                    Animations.Dequeue();
                ExecuteNextAction();
                CurrentFighter++;

                if (CurrentFighter >= FightersByInitiative.Count)
                {
                    CurrentFighter = 0;
                    CurrentExecutionTurn++;
                }
            }

            Executing = false;
            CurrentExecutionTurn = 0;
        }
    }

    private void ProcessPlanningInput()
    {
        if (Input.KeyPressed(Key.W))
        {
            var fighter = FightersByInitiative[CurrentlySelectedShip].GetComponent<FighterComponent>();
            fighter.Actions.Add(new MoveShip(new Vector2(0, -1), 2));
        }
        if (Input.KeyPressed(Key.S))
        {
            var fighter = FightersByInitiative[CurrentlySelectedShip].GetComponent<FighterComponent>();
            fighter.Actions.Add(new MoveShip(new Vector2(0, 1), 2));
        }
        if (Input.KeyPressed(Key.Space))
        {
            Executing = true;
        }
    }
}
