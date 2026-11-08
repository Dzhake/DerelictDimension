using Friflo.EcGui;
using Hexa.NET.ImGui;
using Monod.AssetsModule;
using Monod.ModsModule;

namespace DerelictDimension.Debug;

public static class Settings
{
    public static bool ShowReloadWindow;
    public static bool ShowSettingsWindow;
    public static bool ShowDemoWindow;
    public static bool ShowExplorer;
    public static bool ShowInspector;
    public static bool ShowB2Debug;
    public static bool ShowPerfMonitor;

    public static void Draw()
    {
        var io = ImGui.GetIO();
        if (ShowSettingsWindow) SettingsWindow.Draw();
        if (ShowReloadWindow) DrawReloadWindow(io);
        if (ShowDemoWindow) ImGui.ShowDemoWindow();
        if (ShowExplorer) EcGui.ExplorerWindow();
        if (ShowInspector) EcGui.InspectorWindow();

        if (ShowB2Debug) B2DebugWindow.Draw();
        if (ShowPerfMonitor) PerfMonitor.Draw();
    }

    private static void DrawReloadWindow(ImGuiIOPtr io)
    {
        ImGui.Begin("Reload"u8);
        if (ImGui.Button("Reload world"u8))
            TheGame.I.Reload();
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
