using Hexa.NET.ImGui;
using Monod.ECS.Systems;
using System;
using System.Globalization;

namespace DerelictDimension.Debug;

/// <summary>
/// Heap-allocation-free perf monitor. Every string handed to ImGui is a NUL-terminated
/// UTF-8 span: numbers are formatted into stack buffers, system names are encoded once and cached.
/// </summary>
public static class PerfMonitor
{
    private const int AvgFrameCount = 60;

    public static void Draw()
    {
        ImGui.Begin("Perf monitor"u8);

        ImGui.SeparatorText("Systems"u8);

        DrawGroup(TheGame.LogicSystemRoot);
        DrawGroup(TheGame.DrawSystemRoot);

        ImGui.End();
    }

    private static void DrawGroup(SystemGroup group)
    {
        var perf = group.Perf;
        float groupMs = perf.LastAvgMs(AvgFrameCount);
        long groupMem = perf.LastMemory;

        if (ImGui.TreeNode(group.Name))
        {
            DrawRow(groupMs, groupMem, groupMs, groupMem);

            var children = group.Systems;
            int count = children.Count;
            float avgMs = count > 0 ? groupMs / count : 0f;
            float avgMem = count > 0 ? groupMem / (float)count : 0f;

            for (int n = 0; n < count; n++)
            {
                var system = children[n];
                if (system is SystemGroup innerGroup)
                {
                    DrawGroup(innerGroup);
                }
                else
                {
                    ImGui.Text(system.Name);
                    var systemPerf = system.Perf;
                    DrawRow(systemPerf.LastAvgMs(AvgFrameCount), systemPerf.LastMemory, avgMs, avgMem);
                }
            }
            ImGui.TreePop();
        }
        else
        {
            DrawRow(groupMs, groupMem, groupMs, groupMem);
        }
    }

    /// <summary>Draws "memory  ms" right-aligned; each value is colored against its own average.</summary>
    private static void DrawRow(float ms, long memory, float avgMs, float avgMem)
    {
        // Green (below average) -> yellow (at average) -> red (2x average and above).
        float msRatio = avgMs > 0f ? Math.Clamp(ms / avgMs, 0f, 2f) * 0.5f : 0f;
        float memRatio = avgMem > 0f ? Math.Clamp(memory / avgMem, 0f, 2f) * 0.5f : 0f;

        // Both values formatted into a single stack buffer, laid out as [memory][0][ms][0].
        Span<char> chars = stackalloc char[48];
        Span<byte> utf8 = stackalloc byte[128];

        int len = FormatMemory(memory, chars);
        int memLen = WriteAscii(chars.Slice(0, len), utf8);

        int msOffset = memLen + 1;
        len = FormatMs(ms, chars);
        int msLen = WriteAscii(chars.Slice(0, len), utf8.Slice(msOffset));

        ReadOnlySpan<byte> memText = utf8.Slice(0, memLen);
        ReadOnlySpan<byte> msText = utf8.Slice(msOffset, msLen);

        float gap = ImGui.GetStyle().ItemSpacing.X;
        float memWidth = ImGui.CalcTextSize(memText).X;
        float msWidth = ImGui.CalcTextSize(msText).X;

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - memWidth - gap - msWidth);
        ImGui.TextColored(new(memRatio, 1f - memRatio, 0f, 1f), memText);   // digits only -> printf-safe
        ImGui.SameLine();
        ImGui.TextColored(new(msRatio, 1f - msRatio, 0f, 1f), msText);
    }

    /// <summary>"0.5432 ms"</summary>
    private static int FormatMs(float ms, Span<char> buf)
    {
        ms.TryFormat(buf, out int len, "F4", CultureInfo.InvariantCulture);
        buf[len++] = ' '; buf[len++] = 'm'; buf[len++] = 's';
        return len;
    }

    /// <summary>"512 B", "1.5 KB", "12.3 MB", "1.2 GB"</summary>
    private static int FormatMemory(long bytes, Span<char> buf)
    {
        double value = bytes;
        int unit = 0;
        while (unit < 3 && value >= 1024d) { value /= 1024d; unit++; }

        int len;
        if (unit == 0) bytes.TryFormat(buf, out len, default, CultureInfo.InvariantCulture);
        else value.TryFormat(buf, out len, "F1", CultureInfo.InvariantCulture);

        buf[len++] = ' ';
        if (unit == 0)
        {
            buf[len++] = 'B';
        }
        else
        {
            buf[len++] = "KMG"[unit - 1];
            buf[len++] = 'B';
        }
        return len;
    }

    /// <summary>Copies ASCII chars to bytes and appends the NUL terminator ImGui requires. Returns content length.</summary>
    private static int WriteAscii(ReadOnlySpan<char> src, Span<byte> dst)
    {
        for (int n = 0; n < src.Length; n++)
        {
            dst[n] = (byte)src[n];
        }
        dst[src.Length] = 0;
        return src.Length;
    }
}