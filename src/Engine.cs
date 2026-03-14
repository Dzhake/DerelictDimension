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
using System.Diagnostics;
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

    public Stopwatch stopwatch;

    public Key pressed;
    public Key released;
    //public RebindMenu Rebind;

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
        MainAssetManager.LoadAsset("Fonts/monogram-extended.ttf");
        MainAssetManager.LoadAsset("Fonts/monogram-extended-italic.ttf");
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


        /*Rebind = new(MainUiSystem);
        Rebind.Root.PositionOffset = new(0, 100);*/
    }

    ///<inheritdoc/>
    protected static void LoadFont()
    {
        FontSystem defaultFontSystem = new();
        defaultFontSystem.AddFont(Assets.Get<byte[]>("Fonts/monogram-extended.ttf"));
        FontSystem italicFontSystem = new();
        italicFontSystem.AddFont(Assets.Get<byte[]>("Fonts/monogram-extended-italic.ttf"));
        GlobalFonts.MenuFont = new GenericStashFont(defaultFontSystem.GetFont(36), null!, italicFontSystem.GetFont(36));
    }

    ///<inheritdoc/>
    protected override void UpdateM()
    {
        if (Input.Down(Key.Right))
            offset.X -= 10;
        else if (Input.Down(Key.Left))
            offset.X += 10;

        if (Input.Down(Key.Up))
            offset.Y += 10;
        else if (Input.Down(Key.Down))
            offset.Y -= 10;

        if (Input.Down(0)) text = "Active";
        else text = "Inactive";

        //Rebind.Update();

        if (Input.KeyboardKeysPressed.Count > 0) pressed = Input.KeyboardKeysPressed.ElementAt(0);
        if (Input.KeyboardKeysReleased.Count > 0) released = Input.KeyboardKeysReleased.ElementAt(0);
    }

    /// <inheritdoc/> 
    protected override void DrawM()
    {
        //stopwatch.Stop();
        GenericFont? font = GlobalFonts.MenuFont;
        if (font is null) return;
        Renderer.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);
        Renderer.Clear(Color.Black);
        Vector2 pos = offset.ToVector2();
        pos.X += 10;

        font.DrawString(Renderer.spriteBatch, $"D1: {Input.Down(Key.D1)}, LeftControl: {Input.Down(Key.LeftControl)}, D2: {Input.Down(Key.D2)}", pos, Color.White);
        pos.Y += 50;
        font.DrawString(Renderer.spriteBatch, text, pos, Color.White);
        pos.Y += 50;
        font.DrawString(Renderer.spriteBatch, "Last pressed:", pos, Color.White);
        pos.Y += 30;
        font.DrawString(Renderer.spriteBatch, pressed.ToString(), pos, Color.White);
        pos.Y += 50;

        font.DrawString(Renderer.spriteBatch, "Last released:", pos, Color.White);
        pos.Y += 30;
        font.DrawString(Renderer.spriteBatch, released.ToString(), pos, Color.White);


        Renderer.End();
    }
}
