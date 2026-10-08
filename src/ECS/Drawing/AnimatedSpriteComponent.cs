namespace DerelictDimension.ECS.Drawing;

public record struct AnimatedSpriteComponent : IComponent
{
    public int[] Frames;
    public int FrameNumber;
    /// <summary>
    /// Time since last frame switch.
    /// </summary>
    public float FrameRemainder;
    public float TimeBetweenFrames;

    public string BaseTexturePath;

    public AnimatedSpriteComponent(string baseTexturePath, int[] frames)
    {
        BaseTexturePath = baseTexturePath;
        Frames = frames;
    }
}
