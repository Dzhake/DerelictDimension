using System;

namespace DerelictDimension.ECS.Physics.Components;

[ComponentKey("HitboxComponent")]
[ComponentSymbol("H")]
public struct HitboxComponent : IComponent, IEquatable<HitboxComponent>
{
    public AABB Value;

    public readonly override string? ToString() => Value.ToString();

    public HitboxComponent(AABB value)
    {
        Value = value;
    }

    public HitboxComponent(float centerX, float centerY, float halfWidth, float halfHeight) : this(new(centerX, centerY, halfWidth, halfHeight)) { }

    public static bool operator ==(in HitboxComponent p1, in HitboxComponent p2) => p1.Value == p2.Value;
    public static bool operator !=(in HitboxComponent p1, in HitboxComponent p2) => p1.Value != p2.Value;

    public override readonly int GetHashCode() => Value.GetHashCode();
    public readonly bool Equals(HitboxComponent other) => Value == other.Value;
    public override readonly bool Equals(object? obj) => obj is HitboxComponent otherPos && otherPos.Equals(this);
}
