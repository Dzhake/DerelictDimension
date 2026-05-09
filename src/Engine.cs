using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extended.Font;
using MLEM.Font;
using Monod;
using Monod.AssetsModule;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using Monod.InputModule;
using Monod.ModsModule;
using Monod.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DerelictDimension;

/// <inheritdoc/> 
public class Engine : MonodGame
{
    /// <summary>
    /// Static singleton instance of the <see cref="Engine"/>.
    /// </summary>
    public static readonly Engine Instance = new();

    public string text = "None";

    public string errors = "";

    public Point offset = Point.Zero;

    public Key pressed;
    public Key released;
    public RebindMenu Rebind;

    public HashSet<string> ModsToToggle = new();
    public int Page = 0;

    /// <summary>
    /// Creates a new <see cref="Engine"/>.
    /// </summary>
    public Engine()
    {
        IsMouseVisible = true;
    }

    ///<inheritdoc/>
    protected override void LoadContent()
    {
        MainAssetManager.LoadAsset("Fonts/m6x11plus.ttf");
        LoadFont();
        base.LoadContent();
        Assets.OnReload += LoadFont;

        var jumpIndex = InputActionIndex.Info.AddOrGetValue("Jump");
        var moveLeftIndex = InputActionIndex.Info.AddOrGetValue("Move Left");
        var moveRightIndex = InputActionIndex.Info.AddOrGetValue("Move Right");
        Input.DefaultMap = new()
        {
            {jumpIndex, new([new(Key.D1, KeyModifiers.None)]) },
            {moveLeftIndex, new([new(Key.D2, KeyModifiers.Ctrl), new(Key.D3, KeyModifiers.Ctrl | KeyModifiers.Alt)]) },
            {moveRightIndex, new([]) }
        };


        //Rebind = new(MainUiSystem);
        //Rebind.Root.PositionOffset = new(0, 100);
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

        if (Input.ActionDown((InputActionIndex)0)) text = "Active";
        else text = "Inactive";

        int i = 0;

        if (Input.KeyPressed(Key.D)) Page++;
        else if (Input.KeyPressed(Key.A)) Page--;

        if (Input.KeyPressed(Key.R)) ModManager.EnqueueLoadAllMods();

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

        Rebind?.Update();

        if (Input.KeyboardKeysPressed.Count > 0) pressed = Input.KeyboardKeysPressed.ElementAt(0);
        if (Input.KeyboardKeysReleased.Count > 0) released = Input.KeyboardKeysReleased.ElementAt(0);
    }

    /// <inheritdoc/>
    protected override void DrawM()
    {
        GenericFont? font = GlobalFonts.MenuFont;
        if (font is null) return;
        Renderer.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);
        Renderer.Clear(Color.Black);
        Vector2 pos = offset.ToVector2();
        pos.X += 10;

        /*font.DrawString(Renderer.spriteBatch, $"D1: {Input.KeyDown(Key.D1)}, LeftControl: {Input.KeyDown(Key.LeftControl)}, D2: {Input.KeyDown(Key.D2)}", pos, Color.White);
        pos.Y += 50;
        font.DrawString(Renderer.spriteBatch, text, pos, Color.White);
        pos.Y += 50;
        font.DrawString(Renderer.spriteBatch, "Last pressed:", pos, Color.White);
        pos.Y += 30;
        font.DrawString(Renderer.spriteBatch, pressed.ToString(), pos, Color.White);
        pos.Y += 50;

        font.DrawString(Renderer.spriteBatch, "Last released:", pos, Color.White);
        pos.Y += 30;
        font.DrawString(Renderer.spriteBatch, released.ToString(), pos, Color.White);*/
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


        Renderer.End();
    }
}
