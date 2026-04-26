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

        Input.ActionNames.AddOrGetValue("Jump");
        Input.ActionNames.AddOrGetValue("Move Left");
        Input.ActionNames.AddOrGetValue("Move Right");
        Input.DefaultMap = new()
        {
            {0, new([new(Key.D1, KeyModifiers.None)]) },
            {1, new([new(Key.D2, KeyModifiers.Ctrl), new(Key.D3, KeyModifiers.Ctrl | KeyModifiers.Alt)]) },
            {2, new([]) }
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

        if (Input.ActionDown(0)) text = "Active";
        else text = "Inactive";

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
        foreach (var brokenMod in ModManager.BrokenMods)
        {
            font.DrawString(Renderer.spriteBatch, brokenMod.ManifestPath, pos, Color.Red);
            pos.Y += 16;
            font.DrawString(Renderer.spriteBatch, brokenMod.FailureReason.Message, pos, Color.Orange);
            pos.Y += 32;
        }

        pos.Y += 16;

        int i = 0;

        foreach (var mod in ModManager.Mods.Values)
        {
            Color color;
            switch (mod.Status)
            {
                case ModStatus.Enabled:
                    if (ModsToToggle.Contains(mod.GetName()))
                    {
                        color = Color.Green;
                        break;
                    }
                    color = Color.White;
                    break;

                case ModStatus.Disabled:
                    if (ModsToToggle.Contains(mod.GetName()))
                    {
                        color = Color.DarkSlateBlue;
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
            if (Input.KeyPressed(Key.D0 + i))
            {
                ModsToToggle.ToggleValue(mod.GetName());
            }
            i++;
        }


        Renderer.End();
    }
}
