using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monod;
using Monod.AssetsSystem;
using Monod.AssetsSystem.AssetLoaders;
using Monod.GraphicsSystem;
using Monod.GraphicsSystem.BitmapFonts;
using Monod.InputSystem;
using Monod.LocalizationSystem;
using Monod.Utils.General;
using System;
using System.Text.Json;
using Monod.GraphicsSystem.Fonts;

namespace DerelictDimension;

/// <inheritdoc/> 
public class Engine : MonodGame
{
    /// <summary>
    /// Static singleton instance of the <see cref="Engine"/>.
    /// </summary>
    public static Engine Instance = null!;

    public string text = "None";

    public Point offset = Point.Zero;

    /// <summary>
    /// Creates a new <see cref="Engine"/>.
    /// </summary>
    public Engine()
    {
        Instance = this;
    }

    protected override void LoadContent()
    {
        Renderer.spriteBatch = new SpriteBatch(GraphicsDevice);

        string contentPath = $"{AppContext.BaseDirectory}Content";
        MainAssetManager = new AssetManager(new FileAssetLoader((contentPath)));
        Assets.RegisterAssetManager(MainAssetManager, "");
        MainAssetManager.LoadAsset("THEFONT.png");
        MainAssetManager.LoadAsset("THEFONT_info.json");
        LoadFont();
        Assets.OnReload += LoadFont;
        MainAssetManager.LoadAssets();
    }

    protected void LoadFont()
    {
        Texture2D? fontTexture = Assets.GetOrDefault<Texture2D>(":THEFONT.png");
        string? fontInfo = Assets.GetOrDefault<string>(":THEFONT_info.json");
        if (fontTexture is not null && fontInfo is not null) GlobalFonts.MenuFont = new BitmapFont(fontTexture, JsonSerializer.Deserialize<BitmapFont.Info>(fontInfo, Json.SerializeWithFields));
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

        /*if (font is not null) return;
        Texture2D? fontTexture = Assets.GetOrDefault<Texture2D>(":THEFONT.png");
        string? fontInfo = Assets.GetOrDefault<string>(":THEFONT_info.json");
        if (fontTexture is not null && fontInfo is not null) font = new(fontTexture, JsonSerializer.Deserialize<BitmapFont.Info>(fontInfo, Json.SerializeWithFields));*/
    }

    /// <inheritdoc/> 
    protected override void DrawM()
    {
        IFont? font = GlobalFonts.MenuFont;
        if (font is null) return;
        Renderer.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);
        Renderer.DrawRect(new(0, 0), new(1000, 500), Color.DarkSlateBlue);
        Vector2 pos = offset.ToVector2();
        font.DrawText("Derelict Dimension", pos, Color.LightGreen, scale: new(3));
        pos.Y += 50;
        font.DrawText("(quick brown fox jumps over lazy dog)",  pos, Color.Orange, scale: new(2));
        pos.Y += 50;
        font.DrawText("AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789()/%:,.", pos, Color.LightGoldenrodYellow, scale: new(2));
        pos.Y += 50;
        font.DrawText("0\n1\n2\n3\n4\n5\n6\n7\n8\n9\n:)", pos, Color.Aquamarine);
        pos.Y += 150;
        font.DrawText(text, pos, Color.Red);
        pos.Y += 100;
        font.DrawText(Locale.Get("One"), pos, Color.Blue, scale: new(2,2));
        pos.Y += 40;
        font.DrawText(Locale.Get("Two"), pos, Color.Blue, scale: new(2,2));
        Renderer.End();
    }
}
