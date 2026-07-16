using DerelictDimension.ECS.Ai;
using DerelictDimension.ECS.Ai.Cloud;
using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.Physics;
using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Prefabs;
using DerelictDimension.ECS.Rewinding;
using FontStashSharp;
using Friflo.EcGui;
using MLEM.Extended.Font;
using MLEM.Font;
using Monod;
using Monod.AssetsModule;
using Monod.ECS.DefaultComponents;
using Monod.ECS.Prefabs;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using Monod.InputModule;
using Monod.MathModule;
using Monod.ModsModule;
using Monod.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DerelictDimension;

/// <summary>
/// Entry point of the game, that manages update and render.
/// </summary>
public class TheGame : MonodGame
{
    /// <summary>
    /// Static singleton instance of the <see cref="TheGame"/>.
    /// </summary>
    public static TheGame Instance;

    public string text = "None";

    public string errors = "";

    public Point offset = Point.Zero;

    public Key pressed;
    public Key released;
    public RebindMenu Rebind;

    public HashSet<string> ModsToToggle = [];
    public int Page = 0;

    public static Vector2 GameSize;
    public static Entity entity;
    public static float WantedFPS = 60;


    /// <summary>
    /// Creates a new <see cref="TheGame"/>.
    /// </summary>
    [SetsRequiredMembers]
    public TheGame()
    {
        IsMouseVisible = true;
        Instance = this;
        GameSize = new(1280, 720);
        IsFixedTimeStep = true;
        MaxElapsedTime = TimeSpan.FromMilliseconds(1000f / WantedFPS * 60);
        TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / WantedFPS);
        PrefabsStore = new(PidType.RandomPids);
    }


    protected override void Initialize()
    {
        base.Initialize();
        EcGui.AddExplorerStore("Main", Store);
        EcGui.AddExplorerStore("Prefabs", PrefabsStore);
    }

    public override void CreateStore() => Store = new(PidType.RandomPids);

    ///<inheritdoc/>
    protected override void LoadContent()
    {
        MainAssetManager.LoadAsset("Fonts/m6x11plus.ttf");
        MainAssetManager.LoadAsset("Prefabs/MonstarCannon.prefab.json");
        MainAssetManager.LoadAsset("Prefabs/Platform.prefab.json");

        LoadFont();
        base.LoadContent();

        Renderer.deviceManager.PreferMultiSampling = true;
        //Renderer.DefaultBlendState = Renderer.NonPremultipliedBlend;
        Assets.OnReload += LoadFont;
        InitWorld();
    }

    public void InitWorld()
    {
        ClearStore();

        var cannonPrefab = Assets.Get<PrefabAsset>("Prefabs/MonstarCannon.prefab.json");
        var cannon = cannonPrefab.Instantiate(Store);

        entity = Store.CreateEntity();
        entity.Add(new CloudBehaviour(), new HitboxComponent(0, 0, 100, 30), new SupportComponent(Direction4.Up, -0.05f), new Transform2D(0, 600), new MobileInfoComponent() { AffectedByGravity = false }, new MobileComponent() { Velocity = new(100, 0) }, new MortalComponent(), Tags.Get<FragileTag>());

        var json = entitySerializer.WriteEntity(entity);
        Log.Information($"\n{json}");


        var platformPrefab = Assets.Get<PrefabAsset>("Prefabs/Platform.prefab.json");
        var platform1 = platformPrefab.Instantiate(Store);
        platform1.GetComponent<Transform2D>().Position = new(300, 700);

        platform1 = platformPrefab.Instantiate(Store);
        platform1.GetComponent<Transform2D>().Position = new(1000, 650);

        platform1 = platformPrefab.Instantiate(Store);
        platform1.GetComponent<Transform2D>().Position = new(1200, 600);
        platform1.RemoveTag<SolidTag>();
        platform1.GetComponent<SupportComponent>().Normals = Direction4.Up;

        platform1 = platformPrefab.Instantiate(Store);
        platform1.GetComponent<Transform2D>().Position = new(640, 0);
        platform1.GetComponent<HitboxComponent>().Value.HalfWidth = 640;

        platform1 = platformPrefab.Instantiate(Store);
        platform1.GetComponent<Transform2D>().Position = new(640, 300);
        platform1.GetComponent<HitboxComponent>().Value.HalfWidth = 640;


        entity = Store.CreateEntity(new MobileComponent(), new MobileInfoComponent() { Restitution = new(0, 0) }, new HitboxComponent(0, 0, 30, 50), new Transform2D(300, 100), new BounceableComponent(100, 200, 50), new MortalComponent(), new PlayerAi(), new PlayerAiInfo());

        InitializeSystems();
    }

    private static void ClearStore()
    {
        var commandBuffer = Store.GetCommandBuffer();
        foreach (var entity in Store.Entities)
            commandBuffer.DeleteEntity(entity.Id);
        commandBuffer.Playback();
    }

    public void InitializeSystems()
    {
        LogicSystemRoot.RemoveAllSystems();
        DrawSystemRoot.RemoveAllSystems();

        LogicSystemRoot.Add(new RewindPreUpdateSystem());
        LogicSystemRoot.Add(new CannonSystem());
        LogicSystemRoot.Add(new CloudSystem());
        LogicSystemRoot.Add(new AiPreUpdateSystem());
        LogicSystemRoot.Add(new BunnySystem());
        LogicSystemRoot.Add(new PhysicsSystem());
        LogicSystemRoot.Add(new RewindPostUpdateSystem());
        LogicSystemRoot.Add(new AiPostUpdateSystem());

        DrawSystemRoot.Add(new DrawSystem());
        //DrawSystemRoot.Add(new DrawSpriteSystem());
    }

    public void Reload()
    {
        InitWorld();
        Rewind.RecordedComponents.Clear();
        Rewind.CurrentFrame = 0;
    }

    ///<inheritdoc/>
    protected static void LoadFont()
    {
        FontSystem defaultFontSystem = new();
        defaultFontSystem.AddFont(Assets.Get<byte[]>("Fonts/m6x11plus.ttf"));
        GlobalFonts.MenuFont = new GenericStashFont(defaultFontSystem.GetFont(18));
    }

    ///<inheritdoc/>
    protected override void UpdateM()
    {
        if (Input.KeyDown(Key.Right))
            offset.X -= 10;
        else if (Input.KeyDown(Key.Left))
            offset.X += 10;

        if (Input.KeyDown(Key.Up))
            offset.Y += 10;
        else if (Input.KeyDown(Key.Down))
            offset.Y -= 10;

        if (Input.KeyPressed(Key.R)) Reload();

        //if (Input.ActionDown((InputActionIndex)0)) text = "Active";
        //else text = "Inactive";

        int i = 0;

        if (Input.KeyPressed(Key.D)) Page++;
        else if (Input.KeyPressed(Key.A)) Page--;

        foreach (var mod in ModManager.Mods.Values.Skip(Page * 10).Take(10))
        {
            if (Input.KeyPressed(Key.D0 + i))
                ModsToToggle.ToggleValue(mod.GetName());
            i++;
        }

        if (Input.KeyPressed(Key.Enter) && ModsToToggle.Count != 0)
        {
            ModManager.EnqueueToggleMods(ModsToToggle);
            ModsToToggle.Clear();
        }

        if (Input.KeyPressed(Key.F1)) Settings.ShowDemoWindow = !Settings.ShowDemoWindow;
        if (Input.KeyPressed(Key.F2)) Settings.ShowSettingsWindow = !Settings.ShowSettingsWindow;
        if (Input.KeyPressed(Key.F3)) Settings.ShowInspector = !Settings.ShowInspector;
        if (Input.KeyPressed(Key.F5)) Settings.ShowReloadWindow = !Settings.ShowReloadWindow;

        Rebind?.Update();

        if (Input.KeyboardKeysPressed.Count > 0) pressed = Input.KeyboardKeysPressed.ElementAt(0);
        if (Input.KeyboardKeysReleased.Count > 0) released = Input.KeyboardKeysReleased.ElementAt(0);
        Vector2 mousepos = (Input.MousePos() - Renderer.RenderOffset) / DrawSystem.Upscale;
        if (Input.KeyDown(Key.Mouse1) && !entity.IsNull)
        {
            var data = entity.Data;
            if (data.Has<Transform2D>())
            {
                ref var transform = ref data.Get<Transform2D>();
                Rewind.StoreComponentUpdated(entity.Id, ref transform);
                transform.Position = mousepos;

                if (data.Has<MobileComponent>())
                {
                    ref var mobile = ref data.Get<MobileComponent>();
                    Rewind.StoreComponentUpdated(entity.Id, ref mobile);
                    mobile.Velocity = Vector2.Zero;
                }
            }
        }
        else if (Input.KeyPressed(Key.Mouse2))
        {
            var bunnyPrefab = Assets.Get<PrefabAsset>("Prefabs/Bunny.prefab.json");
            var bunny = bunnyPrefab.Instantiate(Store);
            bunny.GetComponent<Transform2D>().Position = mousepos;
            Rewind.StoreEntityUpdated(bunny, false);
        }
        else if (Input.KeyPressed(Key.Mouse3) && !entity.IsNull)
        {
            var monstarPrefab = Assets.Get<PrefabAsset>("Prefabs/Monstar.prefab.json");
            var monstar = monstarPrefab.Instantiate(Store);
            monstar.GetComponent<Transform2D>().Position = mousepos;
            Rewind.StoreEntityUpdated(monstar, false);
        }



        UpdateLogicSystems();
    }

    /// <inheritdoc/>
    protected override void DrawM()
    {
        GenericFont? font = GlobalFonts.MenuFont;
        Renderer.Clear(new(0, 0, 0));
        UpdateDrawSystems();

        /*Renderer.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);
        Renderer.End();*/
        //DrawModMenu(font, ref transform);
    }

    public override void DrawImGui()
    {
        base.DrawImGui();
        Settings.Draw();
    }

    private void DrawModMenu(GenericFont font, ref Vector2 pos)
    {
        font.DrawString(Renderer.spriteBatch, $"Found all mods: {ModManager.ModsFound}", pos, Color.White);
        pos.Y += 16;

        foreach (var brokenMod in ModManager.BrokenMods)
        {
            font.DrawString(Renderer.spriteBatch, brokenMod.ManifestPath, pos, Color.Red);
            pos.Y += 16;
            font.DrawString(Renderer.spriteBatch, brokenMod.FailureReason.Message, pos, Color.Orange);
            pos.Y += 32;
        }

        pos.Y += 16;

        //font.DrawString(Renderer.spriteBatch, $"Page: {Page}", transform, Color.White);
        //transform.Y += 16;

        int i = 0;

        foreach (var mod in ModManager.Mods.Values.Skip(Page * 10).Take(10))
        {
            Color color;
            switch (mod.Status)
            {
                case ModStatus.Enabled:
                    if (ModsToToggle.Contains(mod.GetName()))
                    {
                        color = Color.DarkSlateBlue;
                        break;
                    }
                    color = Color.White;
                    break;

                case ModStatus.Disabled:
                    if (ModsToToggle.Contains(mod.GetName()))
                    {
                        color = Color.Green;
                        break;
                    }
                    color = Color.Gray;
                    break;

                default:
                    color = Color.Orange;
                    break;
            }
            font.DrawString(Renderer.spriteBatch, $"{i}. {mod.GetName()}", pos, color);
            pos.Y += 16;
            i++;
        }
    }
}
