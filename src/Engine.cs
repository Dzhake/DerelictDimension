using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monod.GraphicsSystem;
using Monod.GraphicsSystem.BitmapFonts;
using Monod.InputSystem;
using Monod.LocalizationSystem;
using Monod.Utils.General;
using Monod;
using Monod.AssetsSystem;

namespace DerelictDimension;

/// <inheritdoc/> 
public class Engine : MonodGame
{
    /// <summary>
    /// Static singleton instance of the <see cref="Engine"/>.
    /// </summary>
    public static Engine Instance = null!;
    
    public BitmapFont? font;

    public string text = "None";

    public Point offset = Point.Zero;

    /// <summary>
    /// Creates a new <see cref="Engine"/>.
    /// </summary>
    public Engine()
    {
        Instance = this;
    }

    /// <inheritdoc/> 
    protected override void UpdateM()
    {
        if (Input.Down(Keys.Right))
            offset.X += 1;
        else if (Input.Down(Keys.Left))
            offset.X -= 1;

        if (Input.Down(Keys.Up))
            offset.Y -= 1;
        else if (Input.Down(Keys.Down))
            offset.Y += 1;

        if (font is not null) return;
        Texture2D? fontTexture = Assets.GetOrDefault<Texture2D>(":THEFONT");
        string? fontInfo = Assets.GetOrDefault<string>(":THEFONT_info");
        if (fontTexture is not null && fontInfo is not null) font = new(fontTexture, JsonSerializer.Deserialize<BitmapFont.Info>(fontInfo, Json.WithFields));
    }

    /// <inheritdoc/> 
    protected override void DrawM()
    {
        if (font is null) return;
        Renderer.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);
        Renderer.DrawRect(new(0, 0), new(1000, 500), Color.DarkSlateBlue);
        Vector2 pos = offset.ToVector2();
        font.DrawText("Derelict Dimension", Renderer.spriteBatch, pos, Color.LightGreen, scale: new(3));
        pos.Y += 50;
        font.DrawText("(quick brown fox jumps over lazy dog)", Renderer.spriteBatch, pos, Color.Orange, scale: new(2));
        pos.Y += 50;
        font.DrawText("AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789()/%:,.", Renderer.spriteBatch, pos, Color.LightGoldenrodYellow, scale: new(2));
        pos.Y += 50;
        font.DrawText("0\n1\n2\n3\n4\n5\n6\n7\n8\n9\n:)", Renderer.spriteBatch, pos, Color.Aquamarine);
        pos.Y += 150;
        font.DrawText(text, Renderer.spriteBatch, pos, Color.Red);
        pos.Y += 100;
        font.DrawText(Locale.Get("One"), Renderer.spriteBatch, pos, Color.Blue, scale: new(2,2));
        pos.Y += 40;
        font.DrawText(Locale.Get("Two"), Renderer.spriteBatch, pos, Color.Blue, scale: new(2,2));
        Renderer.End();
    }
}
