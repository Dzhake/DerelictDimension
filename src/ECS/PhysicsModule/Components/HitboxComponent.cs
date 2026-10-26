using Box2D.NET;
using System;

namespace DerelictDimension.ECS.PhysicsModule.Components;

[ComponentKey("HitboxComponent")]
[ComponentSymbol("H")]
public record struct HitboxComponent : IComponent, IEquatable<HitboxComponent>
{
    public OrientedRectangle Hitbox;
    public bool Collidable;
    public B2ShapeId ShapeId;
    public bool PhysicsDirty;

    public readonly override string? ToString() => $"Hitbox: {Hitbox}, Collidable: {Collidable}";

    public HitboxComponent(OrientedRectangle value)
    {
        Hitbox = value;
        PhysicsDirty = true;
        Collidable = true;
    }

    public HitboxComponent(float centerX, float centerY, float halfWidth, float halfHeight, float angle = 0) : this(new(centerX, centerY, halfWidth, halfHeight, angle)) { }

    public static bool operator ==(in HitboxComponent h1, in HitboxComponent h2) => h1.Hitbox == h2.Hitbox && h1.Collidable == h2.Collidable;
    public static bool operator !=(in HitboxComponent h1, in HitboxComponent h2) => !(h1 == h2);

    public readonly override int GetHashCode() => Hitbox.GetHashCode();
    public readonly bool Equals(HitboxComponent other) => Hitbox == other.Hitbox && Collidable == other.Collidable;
}
