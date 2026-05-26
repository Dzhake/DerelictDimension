using System;
using System.Runtime.CompilerServices;

namespace DerelictDimension.ECS.Battle.Actions;

[InlineArray(3)]
public struct ActionsArray
{
    private IShipAction? element;
    public static int Length = 3;

    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, 3, nameof(index));
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));

        Span<IShipAction?> span = this;
        //shift by 1 to the left
        span[(index + 1)..].CopyTo(span[index..]);
        span[^1] = null;
    }

    public void Add(IShipAction action)
    {
        int index = LastFreeIndex();
        if (index == Length) return; //uhh lol i should prob do something about this
        this[index] = action;
    }

    /// <summary>
    /// Returns the first free index, or <see cref="Length"/> if none found.
    /// </summary>
    /// <param name="startingAt"></param>
    /// <returns></returns>
    public int LastFreeIndex(int startingAt = 0)
    {
        int i = startingAt;
        Span<IShipAction?> span = this;
        while (i < span.Length && span[i] != null) i++;
        return i;
    }
}
