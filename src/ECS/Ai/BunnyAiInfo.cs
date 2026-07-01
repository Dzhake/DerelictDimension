using System;

namespace DerelictDimension.ECS.Ai;

public record struct BunnyAiInfo : IComponent
{
    public Vector2 SmallJumpStrength = new(200);
    public Vector2 BigJumpStrength = new(400);
    public int SmallJumpsBeforeBigJump = 3;
    public float TimeBeforeSmallJump = 0.5f;
    public float TimeBeforeBigJump = 2f;
    public float DelayAfterSmallJump = 0;
    public float DelayAfterBigJump = 1f;
    public float RotationSpeedWhileDead = MathF.PI;

    public BunnyAiInfo()
    {
    }
}
