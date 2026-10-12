using DerelictDimension.ECS.Ai;
using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.Physics;
using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using FontStashSharp;
using Friflo.EcGui;
using MLEM.Extended.Font;
using MLEM.Font;
using Monod;
using Monod.AssetsModule;
using Monod.ECS.DefaultComponents;
using Monod.Graphics;
using Monod.Graphics.ECS.Sprite;
using Monod.Graphics.Fonts;
using Monod.InputModule;
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

        entity = Store.CreateEntity(new MobileComponent(), new MobileInfoComponent() { Restitution = new(0, 0) }, new HitboxComponent(0, 0, 30, 50), new Transform2D(300, 100), new MortalComponent(), new PlayerAi());
        var body = Store.CreateEntity(new Transform2D(0, 0), new AnimatedSpriteComponent("Sprites/Player/body", [0, 1]), new RenderDepth(0.2f), Tags.Get<LookAtMouseTag>());
        entity.AddChild(body);
        var head = Store.CreateEntity(new Transform2D(0, 0), new AnimatedSpriteComponent("Sprites/Player/head", [0, 1]), new RenderDepth(0.1f));
        body.AddChild(head);
        var legs = Store.CreateEntity(new Transform2D(0, 0), new AnimatedSpriteComponent("Sprites/Player/legs", [0, 1, 2, 1, 0, 3, 4, 3], 0.1f), new RenderDepth(0.3f), Tags.Get<ParentVelocityDirTag, LegsTag>());
        entity.AddChild(legs);


        Store.CreateEntity(new Transform2D(500, 500, scaleX: 2), new Sprite2D("Sprites/Wall"), new RenderDepth(0.05f), new NineSliceComponent(new(255, 100)));
        Store.CreateEntity(new Transform2D(800, 300, rotation: (float)Math.PI / 6), new Sprite2D("Sprites/Wall", spriteEffects: SpriteEffects.FlipHorizontally), new RenderDepth(0.05f), new NineSliceComponent(new(30, 200)));
        //var json = entitySerializer.WriteEntity(entity);
        //Log.Information($"\n{json}");

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


        LogicSystemRoot.Add(new PlayerAiSystem());
        LogicSystemRoot.Add(new PhysicsSystem());
        LogicSystemRoot.Add(new ParentVelocityDirSystem());
        LogicSystemRoot.Add(new LegsSystem());
        LogicSystemRoot.Add(new LookAtMouseSystem());

        LogicSystemRoot.Add(new AnimatedSpriteSystem());
        LogicSystemRoot.Add(new UpdateSpriteSystem());

        DrawSystem drawSystem = new();
        drawSystem.Add(new DrawSpriteSystem());
        DrawSystemRoot.Add(drawSystem);
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
        defaultFontSystem.AddFont(Assets.Get<byte[]>("Fonts/m6x11plus"));
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
        /*if (Input.KeyDown(Key.Mouse1) && !entity.IsNull)
        {
            var data = entity.Data;
            if (data.Has<Transform2D>())
            {
                ref var transform = ref data.Get<Transform2D>();
                transform.Position = mousepos;

                if (data.Has<MobileComponent>())
                {
                    ref var mobile = ref data.Get<MobileComponent>();
                    mobile.Velocity = Vector2.Zero;
                }
            }
        }*/



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
