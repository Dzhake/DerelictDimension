using Hexa.NET.ImGui;

namespace DerelictDimension.DevTools.Perf;

public static class PerfMonitor
{
    public static void Draw()
    {
        ImGui.Begin("Perf monitor"u8);

        if (ImGui.BeginTabBar("PerfTabs"u8))
        {
            if (ImGui.BeginTabItem("Systems"u8))
            {
                SystemsMonitor.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Graphics"u8))
            {
                GraphicsMonitor.Draw();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
    }
}