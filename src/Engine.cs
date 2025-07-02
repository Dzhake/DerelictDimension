using System;
using System.Globalization;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoPlus;
using MonoPlus.AssetsSystem;
using MonoPlus.GraphicsSystem;
using MonoPlus.GraphicsSystem.BitmapFonts;
using MonoPlus.InputSystem;
using MonoPlus.LocalizationSystem;
using MonoPlus.ModSystem;
using MonoPlus.TimeSystem;
using MonoPlus.Utils;
using Serilog;

namespace DerelictDimension;

/// <inheritdoc/> 
public class Engine : Game
{
    /// <summary>
    /// Static singleton instance of the <see cref="Engine"/>.
    /// </summary>
    public static Engine? Instance;

    /// <summary>
    /// Main <see cref="AssetsManager"/> for vanilla game.
    /// </summary>
    public static AssetsManager? MainAssetManager;

    public BitmapFont? font;

    public string text = "None";

    public Point offset = Point.Zero;

    /// <summary>
    /// Creates a new <see cref="Engine"/>.
    /// </summary>
    public Engine()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        MonoPlusMain.OnGameCreated(this);
        Instance = this;
        IsFixedTimeStep = false;
        Window.AllowUserResizing = true;
    }

    /// <inheritdoc/> 
    protected override void Initialize()
    {
        MonoPlusMain.OnGameInitialize(this);
        base.Initialize();
    }

    /// <inheritdoc/> 
    protected override void LoadContent()
    {
        Renderer.spriteBatch = new SpriteBatch(GraphicsDevice);

        string contentPath = $"{AppContext.BaseDirectory}Content";
        MainAssetManager = new FileAssetsManager(contentPath);
        if (MainAssetManager is null) throw new InvalidOperationException("Couldn't create MainAssetManager");
        Assets.RegisterAssetManager(MainAssetManager, "");
        MainAssetManager.LoadAssets();
        Log.Information("Started loading content");
    }

    /// <inheritdoc/> 
    protected override void Update(GameTime gameTime)
    {
        Time.Update(gameTime, IsActive);
        if (GraphicsSettings.FocusLossBehaviour < GraphicsSettings.OnFocusLossBehaviour.Eco && !IsActive) return;
        base.Update(gameTime);
        Input.Update();
        MainThread.Update();

        ModManager.Update();
        DevConsole.Update();

        if (Input.Down(Keys.Right))
            offset.X += 1;
        else if (Input.Down(Keys.Left))
            offset.X -= 1;

        if (Input.Down(Keys.Up))
            offset.Y -= 1;
        else if (Input.Down(Keys.Down))
            offset.Y += 1;

        Input.PostUpdate();

        if (font is not null) return;
        Texture2D? fontTexture = Assets.GetOrDefault<Texture2D>(":/THEFONT");
        string? fontInfo = Assets.GetOrDefault<string>(":/THEFONT_info");
        if (fontTexture is not null && fontInfo is not null) font = new(fontTexture, JsonSerializer.Deserialize<BitmapFont.Info>(fontInfo, Json.WithFields));
    }

    /// <inheritdoc/> 
    protected override void Draw(GameTime gameTime)
    {
        if (GraphicsSettings.FocusLossBehaviour < GraphicsSettings.OnFocusLossBehaviour.Eco && !IsActive) return;
        if (font is null) return;
        GraphicsDevice.Clear(Color.Black);

        Renderer.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);
        if (ModManager.InProgress)
        {
            font.DrawText("Loading mods.", Renderer.spriteBatch, Vector2.Zero);
        }
        else
        {
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
        }
        Renderer.End();
        base.Draw(gameTime);

    }
}
