using Hexa.NET.ImGui;
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
        DrawGraphicsMenu();
        ImGui.EndTabBar();

        ImGui.End();
    }

    private static void DrawGraphicsMenu()
    {
        ImGui.Text("Window mode"u8);
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

        ImGui.Separator();

        if (ImGui.InputInt2("Window Size"u8, ref GraphicsSettings.WindowSize.X))
            GraphicsSettings.ApplyWindowSizeChanges();
        if (ImGui.InputInt2("Window Position"u8, ref GraphicsSettings.WindowPosition.X))
            GraphicsSettings.ApplyWindowPositionChanges();
        if (ImGui.Checkbox("Center window"u8, ref GraphicsSettings.CenterWindow))
            GraphicsSettings.ApplyWindowPositionChanges();
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
