using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extended.Font;
using MLEM.Font;
using MLEM.Formatting;
using Monod;
using Monod.AssetsModule;
using Monod.AssetsModule.AssetLoaders;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using Monod.InputModule;
using Monod.InputModule.InputActions;
using Monod.InputModule.Parsing;
using System;

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

    public TokenizedString tokenized;

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
        MainAssetManager.LoadAsset("Fonts/monogram-extended.ttf");
        MainAssetManager.LoadAsset("Fonts/monogram-extended-italic.ttf");
        LoadFont();
        Assets.OnReload += LoadFont;
        MainAssetManager.LoadAssets();

        //Action = new OrAction([new HeldAction(Key.D1), new AndAction([new PressedAction(Key.D2), new DownAction(Key.LeftControl)])]);
        Action = InputActionParser.TryParse(textbox);
        string errorText = (string)textbox.Clone();
        int addedIndex = 0;
        foreach (var error in InputActionParser.Errors)
        {
            errors += $"{error}\n";
        }
    }

    ///<inheritdoc/>
    protected static void LoadFont()
    {
        FontSystem defFontSystem = new();
        defFontSystem.AddFont(Assets.Get<byte[]>("Fonts/monogram-extended.ttf"));
        FontSystem italicFontSystem = new();
        italicFontSystem.AddFont(Assets.Get<byte[]>("Fonts/monogram-extended-italic.ttf"));
        GlobalFonts.MenuFont = new GenericStashFont(defFontSystem.GetFont(12), null!, italicFontSystem.GetFont(12));
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
        GenericFont? font = GlobalFonts.MenuFont;
        if (font is null) return;
        Renderer.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);
        Renderer.DrawRect(new(0, 0), new(1000, 500), Color.DarkSlateBlue);
        Vector2 pos = offset.ToVector2();
        pos.Y += 50;
        pos.X += 50;
        font.DrawString(Renderer.spriteBatch, text, pos, Color.White);
        pos.Y += 50;
        tokenized.Draw(new(), Renderer.spriteBatch, pos, font, Color.White, 1, 0);
        font.DrawString(Renderer.spriteBatch, textbox, pos, Color.White);
        pos.Y += 100;
        font.DrawString(Renderer.spriteBatch, errors, pos, Color.Red);
        Renderer.End();
    }
}
