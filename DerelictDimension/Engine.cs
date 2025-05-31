using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoPlus;
using MonoPlus.AssetsManagement;
using MonoPlus.Graphics;
using MonoPlus.Graphics.BitmapFonts;
using MonoPlus.InputHandling;
using MonoPlus.Modding;
using MonoPlus.Time;
using MonoPlus.Utils;
using MonoPlus.Utils.Collections;
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
        MainAssetManager = ExternalAssetManagerBase.FolderOrZip(contentPath);
        if (MainAssetManager is null) throw new InvalidOperationException("Couldn't create MainAssetManager");
        Assets.RegisterAssetManager(MainAssetManager, "vanilla");
        MainAssetManager.AddListener(SetContent);
        MainAssetManager.ReloadAssets();
        Log.Information("Started loading content");
    }

    protected void SetContent(object[] _)
    {
        texture = Assets.Load<Texture2D>("vanilla:/THEFONT");
        info = Assets.Load<string>("vanilla:/THEFONT_info");
        font = new(texture, JsonSerializer.Deserialize<BitmapFont.Info>(info, Json.WithFields));
    }

    /// <inheritdoc/> 
    protected override void Update(GameTime gameTime)
    {
        Time.Update(gameTime, IsActive);
        if (GraphicsSettings.FocusLossBehaviour < GraphicsSettings.OnFocusLossBehaviour.Eco && !IsActive) return;
        base.Update(gameTime);
        Input.Update();

        ModManager.Update();
        Assets.Update();

        Input.PostUpdate();
    }

    /// <inheritdoc/> 
    protected override void Draw(GameTime gameTime)
    {
        if (GraphicsSettings.FocusLossBehaviour < GraphicsSettings.OnFocusLossBehaviour.Eco && !IsActive) return;
        if (font is null) return;
        GraphicsDevice.Clear(Color.Black);
        Renderer.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
        MultiTaskWithContextManager<ModConfig> modReloadTaskManager = ModLoader.reloadTaskManager;
        if (modReloadTaskManager.InProgress)
        {
            font.DrawText($"Reloading {modReloadTaskManager.Tasks.Count} mods:", Renderer.spriteBatch, Vector2.Zero);
            Vector2 drawPosition = new(0, font.GlyphSize.Y);
            foreach (ModConfig config in modReloadTaskManager.GetContexts())
            {
                font.DrawText(config.Id.ToString(), Renderer.spriteBatch, drawPosition);
                drawPosition.Y += font.GlyphSize.Y;
            }
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