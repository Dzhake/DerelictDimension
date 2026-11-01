using Friflo.Json.Fliox;
using Monod.Graphics.Animations;

namespace DerelictDimension.ECS.Drawing;

public record struct AnimatedSpriteComponent : IComponent
{
    public string AnimId;

    [Ignore]
    public Animation? Anim;

    /// <summary>
    /// Current frame in <see cref="Frames"/>.
    /// </summary>
    public int FrameNumber;
    /// <summary>
    /// Time since last frame switch.
    /// </summary>
    public float FrameRemainder;

    public readonly float EffectiveTimeBetweenFrames => (Anim?.TimeBetweenFrames ?? 1f) / AnimationSpeedMultiplier;

    /// <summary>
    /// Use when gameplay effects are affecting <see cref="TimeBetweenFrames"/>.
    /// </summary>
    public float AnimationSpeedMultiplier = 1;

    public string BaseTexturePath;

    [Ignore]
    public bool InvalidateCurrentTexture = true;

    public AnimatedSpriteComponent() { }

    public AnimatedSpriteComponent(string baseTexturePath, string animId)
    {
        BaseTexturePath = baseTexturePath;
        AnimId = animId;
    }

    public void Reset()
    {
        FrameNumber = 0;
        FrameRemainder = 0;
        InvalidateCurrentTexture = true;
    }

    public void SetAnim(string animId)
    {
        if (animId == AnimId) return;
        AnimId = animId;
        Anim = null;
        Reset();
    }
}
