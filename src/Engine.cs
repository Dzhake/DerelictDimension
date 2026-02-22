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

    public string textbox = "Or(Down(D1), And(Down(LeftControl), Up(D2)))";

    public Point offset = Point.Zero;

    public InputAction? Action;

    public TokenizedString tokenized;

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
        Renderer.spriteBatch = new SpriteBatch(GraphicsDevice);

        string contentPath = $"{AppContext.BaseDirectory}Content";
        MainAssetManager = new AssetManager(new FileAssetLoader((contentPath)));

        Assets.RegisterAssetManager(MainAssetManager, "");
        MainAssetManager.LoadAsset("Fonts/monogram-extended.ttf");
        MainAssetManager.LoadAsset("Fonts/monogram-extended-italic.ttf");
        LoadFont();
        Assets.OnReload += LoadFont;
        MainAssetManager.LoadAssets();

        Parse();
    }

    private void Parse()
    {
        errors = "";
        Action = InputActionParser.Parse(textbox);
        textbox += $"\n{Action}";
        string errorText = textbox; //string is recreated after the first Insert
        int addedIndex = 0;
        string underlineStart = "<u #FF0000>";
        string underlineEnd = "</u>";
        foreach (var error in InputActionParser.Errors)
        {
            errorText = errorText.Insert(error.StartIndex + addedIndex, underlineStart);
            addedIndex += underlineStart.Length;
            errorText = errorText.Insert(error.StartIndex + error.Length + addedIndex, underlineEnd);
            addedIndex += underlineEnd.Length;
            errors += $"{error}\n";
        }

        tokenized = new TextFormatter().Tokenize(GlobalFonts.MenuFont!, errorText);
    }

    ///<inheritdoc/>
    protected static void LoadFont()
    {
        FontSystem defFontSystem = new();
        defFontSystem.AddFont(Assets.Get<byte[]>("Fonts/monogram-extended.ttf"));
        FontSystem italicFontSystem = new();
        italicFontSystem.AddFont(Assets.Get<byte[]>("Fonts/monogram-extended-italic.ttf"));
        GlobalFonts.MenuFont = new GenericStashFont(defFontSystem.GetFont(36), null!, italicFontSystem.GetFont(36));
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

        if (Input.Down(Key.Q))
        {
            textbox = "Or(Down(D1), And(Down(LeftControl), Up(D2)))";
            Parse();
        }
        else if (Input.Down(Key.W))
        {
            textbox = "Or(Down(D1), And(Down(Mouse99), Up(D2)))";
            Parse();
        }
        else if (Input.Down(Key.E))
        {
            textbox = "Or(Down(D1), And(NonExistingAction(LeftControl), Up(D2)))";
            Parse();
        }
        else if (Input.Down(Key.R))
        {
            textbox = "Or(Down(D1), And((LeftControl), Up(D2)))";
            Parse();
        }
        else if (Input.Down(Key.T))
        {
            textbox = "And(Down(D1), Down(D2))";
            Parse();
        }
        else if (Input.Down(Key.Y))
        {
            textbox = "And(oh, no, this, is, wrong!)";
            Parse();
        }
        else if (Input.Down(Key.A))
        {
            textbox = "And(oh, no, this, is, wrong!";
            Parse();
        }
        else if (Input.Down(Key.S))
        {
            textbox = "And oh, no, this, is, wrong!";
            Parse();
        }


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
        Renderer.Clear(Color.Black);
        Vector2 pos = offset.ToVector2();
        pos.X += 10;
        font.DrawString(Renderer.spriteBatch, $"D1: {Input.GetValue(Key.D1)}, LeftControl: {Input.GetValue(Key.LeftControl)}, D2: {Input.GetValue(Key.D2)}", pos, Color.White);
        pos.Y += 50;
        font.DrawString(Renderer.spriteBatch, text, pos, Color.White);
        pos.Y += 50;
        tokenized.Draw(new(), Renderer.spriteBatch, pos, font, Color.White, 1, 0);
        pos.Y += 100;
        font.DrawString(Renderer.spriteBatch, errors, pos, Color.Red);
        pos.Y += 100;
        font.DrawString(Renderer.spriteBatch, $"Total: {MainAssetManager.Loader.TotalAssets}, Loaded: {MainAssetManager.Loader.LoadedAssets}", pos, Color.White);
        Renderer.End();
    }
}
