using System;

namespace DerelictDimension.ECS.Physics.Components;

[ComponentKey("HitboxComponent")]
[ComponentSymbol("H")]
public struct HitboxComponent : IComponent, IEquatable<HitboxComponent>
{
    public AABB Value;
    public bool Collidable = true;

    public readonly override string? ToString() => Value.ToString();

    public HitboxComponent(AABB value)
    {
        Value = value;
    }

    public HitboxComponent(float centerX, float centerY, float halfWidth, float halfHeight) : this(new(centerX, centerY, halfWidth, halfHeight)) { }

    public static bool operator ==(in HitboxComponent h1, in HitboxComponent h2) => h1.Value == h2.Value;
    public static bool operator !=(in HitboxComponent h1, in HitboxComponent h2) => h1.Value != h2.Value;

    public override readonly int GetHashCode() => Value.GetHashCode();
    public readonly bool Equals(HitboxComponent other) => Value == other.Value;
    public override readonly bool Equals(object? obj) => obj is HitboxComponent otherPos && otherPos.Equals(this);
}
