using DerelictDimension.ECS.Drawing;
using Hexa.NET.ImGui;

namespace DerelictDimension.Debug;

public static class B2DebugWindow
{
    public static void Draw()
    {
        ref var draw = ref DrawHitboxesSystem.draw;

        ImGui.Begin("B2 Debug"u8);

        ImGui.Checkbox("Enable box2d's debug draw"u8, ref DrawHitboxesSystem.Enabled);
        ImGui.Separator();

        ImGui.Checkbox("drawContacts"u8, ref draw.drawContacts);
        ImGui.SameLine();
        ImGui.TextDisabled("Option to draw contact points"u8);

        ImGui.Checkbox("drawAnchorA"u8, ref draw.drawAnchorA);
        ImGui.SameLine();
        ImGui.TextDisabled("Draw anchor A for contact points (instead of anchorB)"u8);

        ImGui.Checkbox("drawShapes"u8, ref draw.drawShapes);

        ImGui.Checkbox("drawChainNormals"u8, ref draw.drawChainNormals);
        ImGui.SameLine();
        ImGui.TextDisabled("Option to draw chain shape normals"u8);

        ImGui.Checkbox("drawJoints"u8, ref draw.drawJoints);

        ImGui.Checkbox("drawJointExtras"u8, ref draw.drawJointExtras);
        ImGui.SameLine();
        ImGui.TextDisabled("Option to draw additional information for joints"u8);

        ImGui.Checkbox("drawBounds"u8, ref draw.drawBounds);
        ImGui.SameLine();
        ImGui.TextDisabled("Option to draw the bounding boxes for shapes"u8);

        ImGui.Checkbox("drawMass"u8, ref draw.drawMass);
        ImGui.SameLine();
        ImGui.TextDisabled("Option to draw the mass and center of mass of dynamic bodies"u8);

        ImGui.Checkbox("drawBodyNames"u8, ref draw.drawBodyNames);

        ImGui.Checkbox("drawGraphColors"u8, ref draw.drawGraphColors);
        ImGui.SameLine();
        ImGui.TextDisabled("Option to visualize the graph coloring used for contacts and joints"u8);

        ImGui.Checkbox("drawContactFeatures"u8, ref draw.drawContactFeatures);
        ImGui.SameLine();
        ImGui.TextDisabled("Option to draw contact feature ids"u8);

        ImGui.Checkbox("drawContactNormals"u8, ref draw.drawContactNormals);

        ImGui.Checkbox("drawContactForces"u8, ref draw.drawContactForces);
        ImGui.SameLine();
        ImGui.TextDisabled("Option to draw contact normal forces"u8);

        ImGui.Checkbox("drawFrictionForces"u8, ref draw.drawFrictionForces);
        ImGui.SameLine();
        ImGui.TextDisabled("Option to draw contact friction forces"u8);

        ImGui.Checkbox("drawIslands"u8, ref draw.drawIslands);
        ImGui.SameLine();
        ImGui.TextDisabled("Option to draw islands as bounding boxes"u8);

        ImGui.InputFloat("forceScale"u8, ref draw.forceScale);
        ImGui.SameLine();
        ImGui.TextDisabled("Scale to use when drawing forces"u8);

        ImGui.InputFloat("jointScale"u8, ref draw.jointScale);
        ImGui.SameLine();
        ImGui.TextDisabled("Global scaling for joint drawing"u8);

        ImGui.End();
    }
}
