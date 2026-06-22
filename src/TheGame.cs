using DerelictDimension.ECS.Ai;
using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using FontStashSharp;
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

        InitWorld();
    }

    public void InitWorld()
    {
        ClearStore();

        Store.CreateEntity(new SupportComponent() { Friction = -0.05f }, new MobileComponent() { Velocity = new(10, 0) }, new MobileInfoComponent() { AffectedByGravity = false }, new HitboxComponent(0, 0, 250, 50), new Transform2D(300, 300));
        Store.CreateEntity(new SupportComponent() { Friction = -0.05f, OverrideRestitution = new(0, 1) }, new HitboxComponent(0, 0, 250, 50), new Transform2D(810, 550.5f));
        entity = Store.CreateEntity(new MobileComponent(), new MobileInfoComponent() { Restitution = new(0, 0), FlipOnEdge = true }, new HitboxComponent(0, 0, 30, 50), new Transform2D(300, 100), new PlayerAi(), new BounceableComponent(100, 200, 50));

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
        LogicSystemRoot.Add(new PhysicsSystem());
        LogicSystemRoot.Add(new RewindPostUpdateSystem());
        LogicSystemRoot.Add(new AiPostUpdateSystem());
        LogicSystemRoot.Add(new AiPreUpdateSystem());

        DrawSystemRoot.Add(new DrawSystem());
        //DrawSystemRoot.Add(new DrawSpriteSystem());
    }

    public void Reload()
    {
        InitWorld();
        Rewind.StoredComponents.Clear();
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
        if (Input.KeyPressed(Key.F5)) Settings.ShowReloadWindow = !Settings.ShowReloadWindow;

        Rebind?.Update();

        if (Input.KeyboardKeysPressed.Count > 0) pressed = Input.KeyboardKeysPressed.ElementAt(0);
        if (Input.KeyboardKeysReleased.Count > 0) released = Input.KeyboardKeysReleased.ElementAt(0);
        Vector2 mousepos = (Input.MousePos() - Renderer.RenderOffset) / DrawSystem.Upscale;
        if (Input.KeyDown(Key.Mouse1))
        {
            var data = entity.Data;
            ref var pos = ref data.Get<Transform2D>();
            pos.Position = mousepos;
            ref var mobile = ref data.Get<MobileComponent>();
            mobile.Velocity = Vector2.Zero;
        }
        else if (Input.KeyPressed(Key.Mouse2))
        {
            Entity ent = Store.CreateEntity(new HitboxComponent(0, 0, 30, 15), new Transform2D(mousepos), new BouncyComponent(), new MonstarAi(), new MortalComponent());
            Rewind.Keep(ent);
        }
        else if (Input.KeyPressed(Key.Mouse3))
        {
            //Entity ent = Store.CreateEntity(new ActorComponent() { Hitbox = new(0, 0, 50, 50) }, new Transform(mousepos), new TimelessComponent());
            entity.AddComponent(new TemporaryTimeless());
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
