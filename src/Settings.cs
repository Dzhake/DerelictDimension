using Hexa.NET.ImGui;
using Monod.AssetsModule;
using Monod.Graphics.Settings;
using Monod.ModsModule;

namespace DerelictDimension;

public static class Settings
{
    public static bool ShowReloadWindow;
    public static bool ShowSettingsWindow;
    public static bool ShowDemoWindow;

    public static void Draw()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        if (ShowSettingsWindow) DrawSettingsWindow();
        if (ShowReloadWindow) DrawReloadWindow(io);
        if (ShowDemoWindow) ImGui.ShowDemoWindow();
    }

    private static void DrawSettingsWindow()
    {
        ImGui.Begin("Settings"u8);
        ImGui.BeginTabBar("Tab"u8);
        ImGui.BeginTabItem("Graphics"u8);
        ImGui.EndTabItem();
        GraphicsMenu();
        ImGui.EndTabBar();

        ImGui.End();
    }

    private static void GraphicsMenu()
    {
        WindowModeSelect();
        ImGui.SeparatorText("Display"u8);
        DisplaySelect();
        ImGui.Separator();
        WindowSettings();
    }

    private static void WindowModeSelect()
    {
        ImGui.SeparatorText("Window mode"u8);
        if (ImGui.RadioButton("Fullscreen"u8, GraphicsSettings.windowMode == WindowMode.Fullscreen))
        {
            GraphicsSettings.windowMode = WindowMode.Fullscreen;
            GraphicsSettings.ApplyWindowMode();
        }
        if (ImGui.RadioButton("Windowed"u8, GraphicsSettings.windowMode == WindowMode.Windowed))
        {
            GraphicsSettings.windowMode = WindowMode.Windowed;
            GraphicsSettings.ApplyWindowMode();
        }
        if (ImGui.RadioButton("Borderless"u8, GraphicsSettings.windowMode == WindowMode.Borderless))
        {
            GraphicsSettings.windowMode = WindowMode.Borderless;
            GraphicsSettings.ApplyWindowMode();
        }
        if (ImGui.RadioButton("Maximized"u8, GraphicsSettings.windowMode == WindowMode.Maximized))
        {
            GraphicsSettings.windowMode = WindowMode.Maximized;
            GraphicsSettings.ApplyWindowMode();
        }
    }

    private static void DisplaySelect()
    {
        DisplayInfo[] displays = GraphicsSettings.Displays;
        float listboxHeight = ImGui.GetFrameHeightWithSpacing() * displays.Length;
        if (ImGui.BeginListBox("", new System.Numerics.Vector2(0, listboxHeight)))
        {
            for (int i = 0; i < displays.Length; i++)
            {
                DisplayInfo display = displays[i];
                if (ImGui.Selectable(display.FancyName, i == GraphicsSettings.SelectedDisplay))
                {
                    GraphicsSettings.SelectedDisplay = i;
                    GraphicsSettings.ApplyWindowMode();
                }
            }

            ImGui.EndListBox();
        }
    }

    private static void WindowSettings()
    {
        if (ImGui.Checkbox("VSync"u8, ref GraphicsSettings.VSync))
            GraphicsSettings.ApplyVSync();

        bool windowed = GraphicsSettings.windowMode == WindowMode.Windowed;
        if (!windowed)
        {
            ImGui.BeginDisabled();
            ImGui.Text("Available only in windowed mode"u8);
            ImGui.Spacing();
        }
        ImGui.SeparatorText("Common window sizes"u8);
        ImGui.Text("16x9: "u8);
        WindowSizeSelector(GraphicsSettings.CommonResolutions16x9);
        ImGui.SeparatorText("Window settings"u8);
        if (ImGui.InputInt2("Window Size"u8, ref GraphicsSettings.WindowSize.X))
            GraphicsSettings.ApplyWindowSize();
        if (ImGui.InputInt2("Window Position"u8, ref GraphicsSettings.WindowPosition.X))
            GraphicsSettings.ApplyWindowPosition();
        if (ImGui.Checkbox("Center window"u8, ref GraphicsSettings.CenterWindow))
            GraphicsSettings.ApplyWindowPosition();

        if (ImGui.Checkbox("Lock mouse inside window"u8, ref GraphicsSettings.MouseLock))
            GraphicsSettings.ApplyMouseLock();

        if (!windowed) ImGui.EndDisabled();
    }

    private static void WindowSizeSelector(Point[] sizes)
    {
        for (int i = 0; i < sizes.Length; i++)
        {
            ImGui.SameLine();
            Point size = sizes[i];
            if (ImGui.Button($"{size.X}x{size.Y}"))
            {
                GraphicsSettings.WindowSize = size;
                GraphicsSettings.ApplyWindowSize();
            }
        }
    }

    private static void DrawReloadWindow(ImGuiIOPtr io)
    {
        ImGui.Begin("Reload"u8);
        if (ImGui.Button("Reload world"))
            TheGame.Instance.Reload();
        if (ImGui.Button("Reload asset users"u8))
            Assets.ReloadThisFrame = true;
        if (ImGui.Button("Reload assets"u8))
            foreach (var assetManager in Assets.Managers.Values)
                assetManager.LoadAssets();
        if (ImGui.Button("Reload all mods"u8))
            ModManager.EnqueueLoadAllMods();
        if (ImGui.Button("Reload enabled mods"u8))
            ModManager.EnqueueLoadEnabledMods();


        ImGui.End();
    }
}
