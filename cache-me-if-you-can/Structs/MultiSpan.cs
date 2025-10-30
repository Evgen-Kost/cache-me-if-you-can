namespace cache_me_if_you_can.Structs;

public ref struct MultiSpan(ReadOnlySpan<char> command, ReadOnlySpan<char> key, ReadOnlySpan<char> value)
{
    public ReadOnlySpan<char> Command = command;
    public ReadOnlySpan<char> Key = key;
    public ReadOnlySpan<char> Value = value;

    public static int Length => 3;

    public ReadOnlySpan<char> this[int index]
    {
        readonly get => index switch
        {
            0 => Command,
            1 => Key,
            2 => Value,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };
        set
        {
            switch (index)
            {
                case 0: Command = value; break;
                case 1: Key     = value; break;
                case 2: Value   = value; break;
                default: throw new IndexOutOfRangeException(nameof(index));
            }
        }
    }
}