using DerelictDimension.ECS.Ai;
using DerelictDimension.ECS.Ai.Damage;
using DerelictDimension.ECS.Ai.Visual;
using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.Editor;
using DerelictDimension.ECS.PhysicsModule;
using DerelictDimension.ECS.PhysicsModule.Components;
using DerelictDimension.ECS.Prefabs;
using Friflo.EcGui;
using Monod;
using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Graphics.ECS;
using Monod.Graphics.ECS.Sprite;
using Monod.MathModule;
using Monod.Utils.Exceptions;

namespace DerelictDimension.ECS;

public static class EcsMain
{
    public static SystemRoot LogicSystemRoot;
    public static SystemRoot DrawSystemRoot;

    public static SystemCtx MainCtx;
    public static SystemCtx EditorCtx;
    public static SystemCtx CurrentCtx => GetCtx(CurrentStores);
    public static EntityStore CurrentStore => CurrentCtx.Store;
    public static Stores CurrentStores;


    /// <summary>
    /// Main <see cref="EntityStore"/> of the game.
    /// </summary>
    public static EntityStore MainStore;

    public static EntityStore PrefabsStore;

    public static EntityStore EditorStore;


    public static void Initialize()
    {
        EcsStores.Main = Stores.Info.AddOrGetValue("Main");
        EcsStores.Editor = Stores.Info.AddOrGetValue("Editor");
        CurrentStores = EcsStores.Main;

        MainStore = new(PidType.RandomPids);
        PrefabsStore = new(PidType.RandomPids);
        EditorStore = new(PidType.RandomPids);

        LogicSystemRoot = new("Logic System Root");
        DrawSystemRoot = new("Draw System Root");
        MonodGame.LogicSystemRoot = LogicSystemRoot;
        MonodGame.DrawSystemRoot = DrawSystemRoot;

        MonodGame.PrefabsStore = PrefabsStore;

        MainCtx = new(MainStore, EcsStores.Main);
        MainCtx.MonitorPerf = true;
        EditorCtx = new(EditorStore, EcsStores.Editor);
        EditorCtx.MonitorPerf = true;

        OnCtxChanged();

        EcGui.AddExplorerStore("Main", MainStore);
        EcGui.AddExplorerStore("Prefabs", PrefabsStore);
    }

    public static void Load()
    {
        InitWorld(MainCtx);
        InitWorld(EditorCtx);
        PopulateMainStore();
        InitializeSystems();
        OnCtxChanged();
    }

    public static void InitWorld(SystemCtx ctx)
    {
        ClearStore(ctx.Store);
        RegisterQueries(ctx);
    }

    private static void PopulateMainStore()
    {
        var player = CodePrefabs.CreateHuman(new(3, 1), 0, "Sprites/Player/", out var body, out _, out var leftLeg, out var rightLeg, 0);
        player.Name.value = "Player";
        player.AddComponent(new PlayerAi());
        body.AddTag<LookAtMouseTag>();
        body.GetComponent<PhysicsShapeComponent>().CollisionCategories = CollisionLayer.Player;
        var kickLeg = MainStore.CreateEntity(new EntityName("Kick leg"), new PhysicsShapeComponent(isSensor: true, categories: CollisionLayer.PlayerAttacks, mask: CollisionLayer.Environment | CollisionLayer.Enemy | CollisionLayer.EnemyAttacks), new HitboxComponent(new OrientedRect(-.14f, -.28f, .14f, .38f)) { Collidable = false }, new KickLegComponent(leftLeg.Pid, rightLeg.Pid), new AnimatedSpriteComponent("Sprites/Player/", Anims.HumanLegKick), new Transform2D(), new RenderDepth(0, 20), new DamagerComponent());
        body.AddChild(kickLeg);

        CodePrefabs.CreateHuman(new(3, 4.2f), 0, "Sprites/Swordsman/", out _, out _, out _, out _);
        CodePrefabs.CreateHuman(new(2, 3), 0, "Sprites/Swordsman/", out _, out _, out _, out _);
        CodePrefabs.CreateHuman(new(4, 3), 0, "Sprites/Swordsman/", out _, out _, out _, out _);

        CodePrefabs.CreateDoor(new(3, 4), 0, new(2, 0.1f), "Sprites/WoodenDoor", new("Sprites/WoodenDoorDebris"));


        MainStore.CreateEntity(new Transform2D(5, 5), new Sprite2D("Sprites/Wall"), new RenderDepth(10f), new NineSliceComponent(new(255, 100)), new PhysicsBodyComponent(), new HitboxComponent(new OrientedRect(0, 0, 2.55f, 1)), new PhysicsShapeComponent(categories: CollisionLayer.Walls));
        MainStore.CreateEntity(new Transform2D(8, 3, rotation: (float)Math.PI / 6), new Sprite2D("Sprites/Wall", spriteEffects: SpriteEffects.FlipHorizontally), new RenderDepth(10f), new NineSliceComponent(new(30, 200)), new PhysicsBodyComponent(), new HitboxComponent(new OrientedRect(0, 0, 0.3f, 2)), new PhysicsShapeComponent(categories: CollisionLayer.Walls));
        //var json = entitySerializer.WriteEntity(entity);
        //Log.Information($"\n{json}");
    }

    private static void ClearStore(EntityStore store)
    {
        var commandBuffer = store.GetCommandBuffer();
        foreach (var entity in store.Entities)
            commandBuffer.DeleteEntity(entity.Id);
        commandBuffer.Playback();
    }

    public static void InitializeSystems()
    {
        LogicSystemRoot.RemoveAllSystems();
        DrawSystemRoot.RemoveAllSystems();

        LogicSystemRoot.Add(new SystemGroup("Behaviours")
        {
            new PlayerAiSystem(),
            new KickLegSystem(),
            new PhysicsSystem(),
            new DamageSystem(),
            new ParentVelocityDirSystem(),
            new LegsSystem(),
            new BodySystem(),
            new LookAtMouseSystem(),

            new DamageCleaupSystem(),
        });

        LogicSystemRoot.Add(new SystemGroup("Predraw")
        {
            new AnimatedSpriteSystem(),
            new UpdateSpriteSystem(),
            new YSortSystem(),
        });

        LogicSystemRoot.Add(new SystemGroup("Editor", EcsStores.Editor)
        {
            new EditorSelectionSystem(),
        });

        DrawSystemRoot.Add(new DrawSystem()
        {
            new DrawSpriteSystem(),
            new DrawHitboxesSystem(),
            new SystemGroup("Editor", EcsStores.Editor)
            {
                new EditorSelectionDrawSystem(),
            },
        });
    }

    public static void RegisterQueries(SystemCtx ctx)
    {
        Queries.Register(ctx);
    }

    public static SystemCtx? GetCtx(Stores store)
    {
        if ((store & EcsStores.Main) != Stores.None) return MainCtx;
        if ((store & EcsStores.Editor) != Stores.None) return EditorCtx;
        Guard.InvalidEnumValue(store);
        return null!;
    }

    public static void OnCtxChanged()
    {
        LogicSystemRoot.SetContext(CurrentCtx);
        DrawSystemRoot.SetContext(CurrentCtx);
    }

    public static void SetStore(Stores store)
    {
        CurrentStores = store;
        OnCtxChanged();
    }
}
