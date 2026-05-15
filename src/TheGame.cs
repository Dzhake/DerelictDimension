using DerelictDimension.ECS.Card;
using FontStashSharp;
using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extended.Font;
using MLEM.Font;
using Monod;
using Monod.AssetsModule;
using Monod.ECS.DefaultComponents;
using Monod.Graphics;
using Monod.Graphics.ECS.Sprite;
using Monod.Graphics.Fonts;
using Monod.InputModule;
using Monod.ModsModule;
using Monod.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DerelictDimension;

/// <inheritdoc/>
public class TheGame : MonodGame
{
    /// <summary>
    /// Static singleton instance of the <see cref="TheGame"/>.
    /// </summary>
    public static readonly TheGame Instance = new();

    public string text = "None";

    public string errors = "";

    public Point offset = Point.Zero;

    public Key pressed;
    public Key released;
    public RebindMenu Rebind;

    public HashSet<string> ModsToToggle = new();
    public int Page = 0;

    /// <summary>
    /// Creates a new <see cref="TheGame"/>.
    /// </summary>
    public TheGame()
    {
        IsMouseVisible = true;
    }

    ///<inheritdoc/>
    protected override void LoadContent()
    {
        MainAssetManager.LoadAsset("Fonts/m6x11plus.ttf");
        LoadFont();
        base.LoadContent();
        Assets.OnReload += LoadFont;

        Monod.Utils.Enums.ExtEnumInfo<InputActionIndex> actionsInfo = InputActionIndex.Info;
        CardSystem.LeanLeft = actionsInfo.AddOrGetValue("Lean left");
        CardSystem.LeanRight = actionsInfo.AddOrGetValue("Lean right");
        Input.DefaultMap = new()
        {
            {CardSystem.LeanLeft, new([new(Key.A), new(Key.Left)]) },
            {CardSystem.LeanRight, new([new(Key.D), new(Key.Right)]) },
        };

        Entity ent = Store.CreateEntity();
        ent.Add(new Sprite2D("Card.png"), new Scale2D(8, 8), new Rotation2D(1), new Position2D(0, 500), new CardComponent());


        InitializeSystems();

        //Rebind = new(MainUiSystem);
        //Rebind.Root.PositionOffset = new(0, 100);
    }

    public static void InitializeSystems()
    {
        LogicSystemRoot.Add(new UpdateSpriteSystem());
        LogicSystemRoot.Add(new CardSystem());

        DrawSystemRoot.Add(new DrawSpriteSystem());
    }

    ///<inheritdoc/>
    protected static void LoadFont()
    {
        FontSystem defaultFontSystem = new();
        defaultFontSystem.AddFont(Assets.Get<byte[]>("Fonts/m6x11plus.ttf"));
        GlobalFonts.MenuFont = new GenericStashFont(defaultFontSystem.GetFont(18));
    }

    ///<inheritdoc/>
    protected override void UpdateM()
    {
        if (Input.KeyDown(Key.Right))
            offset.X -= 10;
        else if (Input.KeyDown(Key.Left))
            offset.X += 10;

        if (Input.KeyDown(Key.Up))
            offset.Y += 10;
        else if (Input.KeyDown(Key.Down))
            offset.Y -= 10;

        if (Input.ActionDown((InputActionIndex)0)) text = "Active";
        else text = "Inactive";

        int i = 0;

        if (Input.KeyPressed(Key.D)) Page++;
        else if (Input.KeyPressed(Key.A)) Page--;

        if (Input.KeyPressed(Key.R)) ModManager.EnqueueLoadAllMods();

        foreach (var mod in ModManager.Mods.Values.Skip(Page * 10).Take(10))
        {
            if (Input.KeyPressed(Key.D0 + i))
                ModsToToggle.ToggleValue(mod.GetName());
            i++;
        }

        if (Input.KeyPressed(Key.Enter) && ModsToToggle.Count != 0)
        {
            ModManager.EnqueueToggleMods(ModsToToggle);
            ModsToToggle.Clear();
        }

        Rebind?.Update();

        if (Input.KeyboardKeysPressed.Count > 0) pressed = Input.KeyboardKeysPressed.ElementAt(0);
        if (Input.KeyboardKeysReleased.Count > 0) released = Input.KeyboardKeysReleased.ElementAt(0);

        UpdateLogicSystems();
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

        //DrawModMenu(font, ref pos);

        UpdateRenderSystems();

        Renderer.End();
    }

    private void DrawModMenu(GenericFont font, ref Vector2 pos)
    {
        font.DrawString(Renderer.spriteBatch, $"Found all mods: {ModManager.ModsFound}", pos, Color.White);
        pos.Y += 16;

        foreach (var brokenMod in ModManager.BrokenMods)
        {
            font.DrawString(Renderer.spriteBatch, brokenMod.ManifestPath, pos, Color.Red);
            pos.Y += 16;
            font.DrawString(Renderer.spriteBatch, brokenMod.FailureReason.Message, pos, Color.Orange);
            pos.Y += 32;
        }

        pos.Y += 16;

        //font.DrawString(Renderer.spriteBatch, $"Page: {Page}", pos, Color.White);
        //pos.Y += 16;

        int i = 0;

        foreach (var mod in ModManager.Mods.Values.Skip(Page * 10).Take(10))
        {
            Color color;
            switch (mod.Status)
            {
                case ModStatus.Enabled:
                    if (ModsToToggle.Contains(mod.GetName()))
                    {
                        color = Color.DarkSlateBlue;
                        break;
                    }
                    color = Color.White;
                    break;

                case ModStatus.Disabled:
                    if (ModsToToggle.Contains(mod.GetName()))
                    {
                        color = Color.Green;
                        break;
                    }
                    color = Color.Gray;
                    break;

                default:
                    color = Color.Orange;
                    break;
            }
            font.DrawString(Renderer.spriteBatch, $"{i}. {mod.GetName()}", pos, color);
            pos.Y += 16;
            i++;
        }
    }
}
