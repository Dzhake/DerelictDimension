using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoPlus.Graphics;
using System.Globalization;
using MonoPlus;
using MonoPlus.AssetsManagement;
using MonoPlus.InputHandling;
using MonoPlus.Modding;
using MonoPlus.Time;
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
    /// Main <see cref="AssetManager"/> for vanilla game.
    /// </summary>
    public static AssetManager? MainAssetManager;

    
    public Effect? effect;

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
        MainAssetManager = ExternalAssetManagerBase.FolderOrZip(contentPath);
        if (MainAssetManager is null) throw new InvalidOperationException("Couldn't create MainAssetManager!");
        Assets.RegisterAssetManager(MainAssetManager, "vanilla");
        MainAssetManager.PreloadAssets();
        Log.Information("Started loading content");
    }

    /// <inheritdoc/> 
    protected override void Update(GameTime gameTime)
    {
        Time.Update(gameTime, IsActive);
        if (GraphicsSettings.PauseOnFocusLoss && !IsActive) return;
        base.Update(gameTime);
        Input.Update();

        ModManager.Update();

        Input.PostUpdate();
    }

    /// <inheritdoc/> 
    protected override void Draw(GameTime gameTime)
    {
        if (GraphicsSettings.PauseOnFocusLoss && !IsActive) return;
        GraphicsDevice.Clear(Color.Black);
        Renderer.Begin(SpriteSortMode.Immediate, effect: effect);
        Renderer.DrawRect(new(0,0), new(1000, 500), Color.DarkBlue);
        Renderer.End();
        base.Draw(gameTime);
    }
}