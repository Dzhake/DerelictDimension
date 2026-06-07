using DerelictDimension.ECS;
using DerelictDimension.ECS.Physics;
using DerelictDimension.ECS.Rewinding;
using FontStashSharp;
using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;
using MLEM.Extended.Font;
using MLEM.Font;
using Monod;
using Monod.AssetsModule;
using Monod.ECS.DefaultComponents;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using Monod.InputModule;
using Monod.ModsModule;
using Monod.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DerelictDimension;

/// <inheritdoc/>
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

    public HashSet<string> ModsToToggle = new();
    public int Page = 0;

    public static Vector2 GameSize;
    public static Entity entity;
    public static int WantedFPS = 60;

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
        TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / WantedFPS);
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    ///<inheritdoc/>
    protected override void LoadContent()
    {
        MainAssetManager.LoadAsset("Fonts/m6x11plus.ttf");
        LoadFont();
        base.LoadContent();
        Renderer.deviceManager.PreferMultiSampling = true;
        Renderer.DefaultBlendState = Renderer.NonPremultipliedBlend;
        Assets.OnReload += LoadFont;

        Monod.Utils.Enums.ExtEnumInfo<InputActionIndex> actionsInfo = InputActionIndex.Info;
        UpdateCardSystem.LeanLeft = actionsInfo.AddOrGetValue("Lean left");
        UpdateCardSystem.LeanRight = actionsInfo.AddOrGetValue("Lean right");
        Input.DefaultMap = new()
        {
            {UpdateCardSystem.LeanLeft, new([new(Key.A), new(Key.Left)]) },
            {UpdateCardSystem.LeanRight, new([new(Key.D), new(Key.Right)]) },
        };

        //Store.CreateEntity(new Sprite2D("Sprites/CardBg.png"), new Position2D(GameSize.X / 2, GameSize.Y / 2), Tags.Get<GameLayerTag>());
        //Store.CreateEntity(new Sprite2D("Sprites/Spaceship.png"), Tags.Get<GameLayerTag>());
        Store.CreateEntity(new SolidComponent() { Hitbox = new(0, 0, 500, 100) }, new Position2D(100, 500));
        Store.CreateEntity(new SolidComponent() { Hitbox = new(0, 0, 500, 100) }, new Position2D(610, 500.5f));
        entity = Store.CreateEntity(new ActorComponent() { Hitbox = new(0, 0, 100, 100) }, new Position2D(300, 100), new PlayerControlledComponent(), new TimelessComponent());


        InitializeSystems();

        //Rebind = new(MainUiSystem);
        //Rebind.Root.PositionOffset = new(0, 100);aaaaaaaaaaaaaaa
    }

    public void InitializeSystems()
    {
        LogicSystemRoot.Add(new RewindPreUpdateSystem());
        LogicSystemRoot.Add(new PhysicsSystem());
        LogicSystemRoot.Add(new RewindPostUpdateSystem());

        DrawSystemRoot.Add(new DrawSystem());
        //DrawSystemRoot.Add(new DrawSpriteSystem());
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
        if (Input.KeyPressed(Key.F5)) Settings.ShowReloadWindow = !Settings.ShowReloadWindow;

        Rebind?.Update();

        if (Input.KeyboardKeysPressed.Count > 0) pressed = Input.KeyboardKeysPressed.ElementAt(0);
        if (Input.KeyboardKeysReleased.Count > 0) released = Input.KeyboardKeysReleased.ElementAt(0);
        if (Input.KeyDown(Key.Mouse1))
        {
            var data = entity.Data;
            ref var pos = ref data.Get<Position2D>();
            pos.Value = Input.MousePos();
            ref var actor = ref data.Get<ActorComponent>();
            actor.Velocity = Vector2.Zero;
        }
        else if (Input.KeyPressed(Key.Mouse2))
        {
            Store.CreateEntity(new ActorComponent() { Hitbox = new(0, 0, 100, 100) }, new Position2D(Input.MousePos()));
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
        //DrawModMenu(font, ref pos);
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

        //font.DrawString(Renderer.spriteBatch, $"Page: {Page}", pos, Color.White);
        //pos.Y += 16;

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
