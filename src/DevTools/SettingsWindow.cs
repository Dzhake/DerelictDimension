using Hexa.NET.ImGui;
using Monod.Graphics.Settings;

namespace DerelictDimension.Debug;

public static class SettingsWindow
{
    public static void Draw()
    {
        ImGui.Begin("Settings"u8);
        ImGui.BeginTabBar("Tab"u8);
        if (ImGui.BeginTabItem("Graphics"u8))
        {
            GraphicsMenu();
            ImGui.EndTabItem();
        }
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
        if (ImGui.RadioButton("Exclusive Fullscreen"u8, GraphicsSettings.windowMode == WindowMode.ExclusiveFullscreen))
        {
            GraphicsSettings.windowMode = WindowMode.ExclusiveFullscreen;
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
        var displays = GraphicsSettings.Displays;
        float listboxHeight = ImGui.GetFrameHeightWithSpacing() * displays.Length;
        if (ImGui.BeginListBox("", new System.Numerics.Vector2(0, listboxHeight)))
        {
            for (int i = 0; i < displays.Length; i++)
            {
                var display = displays[i];
                if (ImGui.Selectable(display.FancyName, i == GraphicsSettings.SelectedDisplay))
                {
                    GraphicsSettings.SelectedDisplay = i;
                    GraphicsSettings.ApplyWindowMode();
                }
            }

            ImGui.EndListBox();
        }

        if (ImGui.Button("Refresh displays list"u8))
            GraphicsSettings.RefreshDisplays();
    }

    private static void WindowSettings()
    {
        if (ImGui.Checkbox("VSync"u8, ref GraphicsSettings.VSync))
            GraphicsSettings.ApplyVSync();

        bool windowed = GraphicsSettings.windowMode == WindowMode.Windowed;

        if (!windowed)
            ImGui.BeginDisabled();
        ImGui.SeparatorText("Windowed mode settings");
        ImGui.Text("Common window sizes:"u8);
        ImGui.Text("16x9: "u8);
        WindowSizeSelector(GraphicsSettings.CommonResolutions16x9);

        if (ImGui.InputInt2("Window Size"u8, ref GraphicsSettings.WindowSize.X))
            GraphicsSettings.ApplyWindowSize();
        if (ImGui.InputInt2("Window Position"u8, ref GraphicsSettings.WindowPosition.X))
            GraphicsSettings.ApplyWindowPosition();
        if (ImGui.Checkbox("Center window"u8, ref GraphicsSettings.CenterWindow))
            GraphicsSettings.ApplyWindowPosition();

        if (!windowed) ImGui.EndDisabled();

        ImGui.SeparatorText("General window settings"u8);
        if (ImGui.Checkbox("Lock mouse inside window"u8, ref GraphicsSettings.MouseLock))
            GraphicsSettings.ApplyMouseLock();
    }

    private static void WindowSizeSelector(Point[] sizes)
    {
        for (int i = 0; i < sizes.Length; i++)
        {
            ImGui.SameLine();
            var size = sizes[i];
            if (ImGui.Button($"{size.X}x{size.Y}"))
            {
                GraphicsSettings.WindowSize = size;
                GraphicsSettings.ApplyWindowSize();
            }
        }
    }
}
