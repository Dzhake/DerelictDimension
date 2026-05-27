using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Monod.AssetsModule;
using Monod.Graphics;
using Monod.Graphics.Settings;

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
            GraphicsSettings.ApplyWindowModeChanges();
        }
        if (ImGui.RadioButton("Windowed"u8, GraphicsSettings.windowMode == WindowMode.Windowed))
        {
            GraphicsSettings.windowMode = WindowMode.Windowed;
            GraphicsSettings.ApplyWindowModeChanges();
        }
        if (ImGui.RadioButton("Borderless"u8, GraphicsSettings.windowMode == WindowMode.Borderless))
        {
            GraphicsSettings.windowMode = WindowMode.Borderless;
            GraphicsSettings.ApplyWindowModeChanges();
        }
        if (ImGui.RadioButton("Maximized"u8, GraphicsSettings.windowMode == WindowMode.Maximized))
        {
            GraphicsSettings.windowMode = WindowMode.Maximized;
            GraphicsSettings.ApplyWindowModeChanges();
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
                    GraphicsSettings.ApplyWindowModeChanges();
                }
            }

            ImGui.EndListBox();
        }
    }

    private static void WindowSettings()
    {
        ImGui.SeparatorText("Common window sizes");
        ImGui.Text("16x9: "u8);
        WindowSizeSelector(GraphicsSettings.CommonResolutions16x9);
        ImGui.Text("4x3: "u8);
        WindowSizeSelector(GraphicsSettings.CommonResolutions4x3);
        ImGui.SeparatorText("Window settings"u8);
        if (ImGui.InputInt2("Window Size"u8, ref GraphicsSettings.WindowSize.X))
            GraphicsSettings.ApplyWindowSizeChanges();
        if (ImGui.InputInt2("Window Position"u8, ref GraphicsSettings.WindowPosition.X))
            GraphicsSettings.ApplyWindowPositionChanges();
        if (ImGui.Checkbox("Center window"u8, ref GraphicsSettings.CenterWindow))
            GraphicsSettings.ApplyWindowPositionChanges();
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
                GraphicsSettings.ApplyWindowSizeChanges();
            }
        }
    }

    private static void DrawReloadWindow(ImGuiIOPtr io)
    {
        ImGui.Begin("Reload"u8);
        if (ImGui.Button("Reload asset users"u8))
            Assets.ReloadThisFrame = true;
        if (ImGui.Button("Reload assets"u8))
            foreach (var assetManager in Assets.Managers.Values)
                assetManager.LoadAssets();

        ImGui.End();
    }
}
