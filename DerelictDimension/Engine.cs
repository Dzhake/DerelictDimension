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

public class Engine : Game
{
    public static Engine? Instance;
    public static AssetManager? MainAssetManager;
    public Effect? effect;

    public Engine()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        MonoPlusMain.OnGameCreated(this);
        Instance = this;
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        MonoPlusMain.OnGameInitialize(this);
        base.Initialize();
    }

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

    protected override void Update(GameTime gameTime)
    {
        if (GraphicsSettings.PauseOnFocusLoss && !IsActive) return;
        base.Update(gameTime);
        Input.Update();
        Time.Update(gameTime);

        ModManager.Update();

        Input.PostUpdate();
    }

    
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