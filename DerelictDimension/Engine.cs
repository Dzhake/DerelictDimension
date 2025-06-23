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
        Assets.RegisterAssetManager(MainAssetManager, "vanilla");
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
    }

    /// <inheritdoc/> 
    protected override void Draw(GameTime gameTime)
    {
        //Console.ReadLine();
        
        if (GraphicsSettings.FocusLossBehaviour < GraphicsSettings.OnFocusLossBehaviour.Eco && !IsActive) return;
        if (font is null) return;
        GraphicsDevice.Clear(Color.Black);
        Renderer.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
        if (ModManager.InProgress)
        {
            font.DrawText($"Loading mods.", Renderer.spriteBatch, Vector2.Zero);
        }
        else
        {
            Renderer.DrawRect(new(0, 0), new(1000, 500), Color.DarkSlateBlue);
            font.DrawText("Derelict Dimension", Renderer.spriteBatch, Vector2.Zero, Color.LightGreen, scale: new(3));
            font.DrawText("(quick, brown, fox jumps over lazy dog)", Renderer.spriteBatch, new(0, 50), Color.Orange, scale: new(2));
        }
        Renderer.End();
        base.Draw(gameTime);
    }
}
