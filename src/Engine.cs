using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monod;
using Monod.AssetsModule;
using Monod.AssetsModule.AssetLoaders;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using Monod.Graphics.Fonts.Bitmap;
using Monod.InputModule;
using Monod.InputModule.InputActions;
using Monod.InputModule.Parsing;
using Monod.Shared;
using System;
using System.Text.Json;

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

    public string textbox = "Or(Pressed(D1), And(Down(Ctrl), Pressed(D2)))";

    public Point offset = Point.Zero;

    public InputAction? Action;

    /// <summary>
    /// Creates a new <see cref="Engine"/>.
    /// </summary>
    public Engine()
    {
    }

    ///<inheritdoc/>
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

        Action = new OrAction([new HeldAction(Key.D1), new AndAction([new PressedAction(Key.D2), new DownAction(Key.LeftControl)])]);
        Action = InputActionParser.TryParse(textbox);
        foreach (var error in InputActionParser.Errors) errors += $"{error}\n";
    }

    ///<inheritdoc/>
    protected static void LoadFont()
    {
        Texture2D? fontTexture = Assets.GetOrDefault<Texture2D>(":THEFONT.png");
        string? fontInfo = Assets.GetOrDefault<string>(":THEFONT_info.json");
        if (fontTexture is not null && fontInfo is not null) GlobalFonts.MenuFont = new BitmapFont(fontTexture, JsonSerializer.Deserialize<BitmapFont.Info>(fontInfo, Json.SerializeWithFields));
    }

    ///<inheritdoc/>
    protected override void UpdateM()
    {
        if (Input.Down(Key.Right))
            offset.X += 1;
        else if (Input.Down(Key.Left))
            offset.X -= 1;

        if (Input.Down(Key.Up))
            offset.Y -= 1;
        else if (Input.Down(Key.Down))
            offset.Y += 1;

        if (Action?.IsActive(0) ?? false) text = "Active";
        else text = "Inactive";

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
        pos.Y += 50;
        pos.X += 50;
        font.DrawText(textbox, pos, Color.White, scale: new(2));
        pos.Y += 50;
        font.DrawText(text, pos, Color.White, scale: new(3));
        pos.Y += 50;
        font.DrawText(errors, pos, Color.Red, scale: new(2));
        Renderer.End();
    }
}
