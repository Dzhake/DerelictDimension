using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoPlus.Graphics;
using System.Globalization;
using System.Threading.Tasks;
using MonoPlus.Assets;
using MonoPlus.Input;
using MonoPlus.Time;

namespace DerelictDimension;

public class Engine : Game
{
    public static Engine? Instance;
    public static AssetManager? MainAssetManager;
    public static Texture2D? idk;

    public Engine()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        Renderer.OnGameCreated(this);
        Instance = this;
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        Renderer.Initialize(this);
        Input.Initialize(this);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        MainAssetManager = new FileSystemAssetManager($"{AppContext.BaseDirectory}Content/");
        Assets.RegisterAssetManager(MainAssetManager, "vanilla");
        Renderer.spriteBatch = new SpriteBatch(GraphicsDevice);
        MainAssetManager.PreloadAssetsAsync();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GraphicsSettings.PauseOnFocusLoss && !IsActive) return;
        base.Update(gameTime);
        Input.Update();
        Time.Update(gameTime);
        Input.PostUpdate();
    }

    
    protected override void Draw(GameTime gameTime)
    {
        if (GraphicsSettings.PauseOnFocusLoss && !IsActive) return;
        GraphicsDevice.Clear(Color.Black);
        Renderer.Begin();
        if (idk is not null) Renderer.DrawTexture(idk, Vector2.Zero, Color.White);
        Renderer.End();
        base.Draw(gameTime);
    }
}