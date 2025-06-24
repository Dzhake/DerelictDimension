using System;
using System.Globalization;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoPlus;
using MonoPlus.AssetsSystem;
using MonoPlus.GraphicsSystem;
using MonoPlus.GraphicsSystem.BitmapFonts;
using MonoPlus.InputSystem;
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


    public Texture2D? texture;
    public string? info;
    public BitmapFont? font;

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
            //Renderer.DrawRect(new(0, 0), new(1000, 500), Color.DarkSlateBlue);
            font.DrawText("Derelict Dimension", Renderer.spriteBatch, Vector2.Zero, Color.LightGreen, scale: new(3));
            font.DrawText("(quick brown fox jumps over lazy dog)", Renderer.spriteBatch, new(0, 50), Color.Orange, scale: new(2));
            font.DrawText("AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789()/%:,.", Renderer.spriteBatch, new(0, 100), Color.LightGoldenrodYellow, scale: new(2));
            font.DrawText("0\n1\n2\n3\n4\n5\n6\n7\n8\n9\n:)", Renderer.spriteBatch, new(0, 150), Color.Aquamarine);
        }
        Renderer.End();
        base.Draw(gameTime);
    }
}
