using Hexa.NET.ImGui;
using Monod.Graphics;

namespace DerelictDimension.DevTools.Perf;

public static class GraphicsMonitor
{
    public static void Draw()
    {
        var metrics = Renderer.device.Metrics;

        ImGui.Text("ClearCount: "u8);
        ImGui.SameLine();
        ImGui.Text(metrics.ClearCount.ToString("G4"));

        ImGui.Text("DrawCount: "u8);
        ImGui.SameLine();
        ImGui.Text(metrics.DrawCount.ToString("G4"));

        ImGui.Text("PixelShaderCount: "u8);
        ImGui.SameLine();
        ImGui.Text(metrics.PixelShaderCount.ToString("G4"));

        ImGui.Text("PrimitiveCount: "u8);
        ImGui.SameLine();
        ImGui.Text(metrics.PrimitiveCount.ToString("G4"));

        ImGui.Text("SpriteCount: "u8);
        ImGui.SameLine();
        ImGui.Text(metrics.SpriteCount.ToString("G4"));

        ImGui.Text("TargetCount: "u8);
        ImGui.SameLine();
        ImGui.Text(metrics.TargetCount.ToString("G4"));

        ImGui.Text("TextureCount: "u8);
        ImGui.SameLine();
        ImGui.Text(metrics.TextureCount.ToString("G4"));

        ImGui.Text("VertexShaderCount: "u8);
        ImGui.SameLine();
        ImGui.Text(metrics.VertexShaderCount.ToString("G4"));
    }
}
