using DerelictDimension.Debug;
using DerelictDimension.ECS;
using DerelictDimension.ECS.Rewinding;
using FontStashSharp;
using MLEM.Extended.Font;
using MLEM.Font;
using Monod;
using Monod.AssetsModule;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using Monod.InputModule;
using Monod.ModsModule;
using SoLoud;
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

    public Wav? LightPiano;
    public Wav? DarkPiano;
    public uint LightPianoH;
    public uint DarkPianoH;



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
        InputActions.Initialize();
    }

    public override void InitializeEcs()
    {
        EcsMain.Initialize();
    }

    ///<inheritdoc/>
    protected override void LoadContent()
    {
        MainAssetManager.LoadAsset("Fonts/m6x11plus.ttf");

        LoadFont();
        base.LoadContent();

        Renderer.deviceManager.PreferMultiSampling = true;
        Assets.OnLoad += LoadFont;
        Assets.OnLoad += OnAssetsLoaded;
        EcsMain.Load();
    }


    public void Reload()
    {
        EcsMain.Load();
        Rewind.RecordedComponents.Clear();
        Rewind.CurrentFrame = 0;
    }

    public void OnAssetsLoaded()
    {
        LightPiano = Assets.Get<Wav>("Audio/LightPiano");
        LightPiano.setLooping(1);
        //LightPianoH = soloud.play(LightPiano);

        DarkPiano = Assets.Get<Wav>("Audio/DarkPiano");
        DarkPiano.setLooping(1);
        //DarkPianoH = soloud.play(DarkPiano);
    }

    ///<inheritdoc/>
    protected static void LoadFont()
    {
        FontSystem defaultFontSystem = new();
        byte[] font = Assets.Get<byte[]>("Fonts/m6x11plus");
        defaultFontSystem.AddFont(font);
        GlobalFonts.MenuFont = new GenericStashFont(defaultFontSystem.GetFont(18));
        GlobalFonts.ShapeMenuFont = new(font);
    }

    ///<inheritdoc/>
    protected override void UpdateM()
    {
        if (Input.KeyPressed(Key.R)) Reload();

        /* Mod manager:
         * 
         * int i = 0;

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
        }*/

        if (Input.KeyPressed(Key.F1)) Settings.ShowDemoWindow = !Settings.ShowDemoWindow;
        if (Input.KeyPressed(Key.F2)) Settings.ShowSettingsWindow = !Settings.ShowSettingsWindow;
        if (Input.KeyPressed(Key.F3)) { if (Input.CurState.Ctrl) Settings.ShowInspector = !Settings.ShowInspector; else Settings.ShowExplorer = !Settings.ShowExplorer; }
        if (Input.KeyPressed(Key.F4)) Settings.ShowB2Debug = !Settings.ShowB2Debug;
        if (Input.KeyPressed(Key.F5)) Settings.ShowReloadWindow = !Settings.ShowReloadWindow;
        if (Input.KeyPressed(Key.F6)) Settings.ShowPerfMonitor = !Settings.ShowPerfMonitor;

        if (Input.KeyPressed(Key.M))
        {
            float lightPianoVol = soloud.getVolume(LightPianoH);
            soloud.fadeVolume(LightPianoH, 1 - lightPianoVol, 0.5);
        }

        Rebind?.Update();

        if (Input.KeyboardKeysPressed.Count > 0) pressed = Input.KeyboardKeysPressed.ElementAt(0);
        if (Input.KeyboardKeysReleased.Count > 0) released = Input.KeyboardKeysReleased.ElementAt(0);

        UpdateLogicSystems();
    }

    /// <inheritdoc/>
    protected override void DrawM()
    {
        UpdateDrawSystems();
        //GenericFont? font = GlobalFonts.MenuFont;
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
