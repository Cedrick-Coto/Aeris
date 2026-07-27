using System.Diagnostics;

namespace Aeris.Engine;

public readonly record struct EntityId(uint Value)
{
    public static EntityId Invalid => new(0);

    public bool IsInvalid => Value == 0;

    public override string ToString() => $"Entity({Value})";
}
