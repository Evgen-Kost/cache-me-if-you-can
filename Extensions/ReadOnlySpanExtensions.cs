namespace cache_me_if_you_can.Extensions;

public static class ReadOnlySpanExtensions
{
    // проверка на цифры
    public static bool ContainsDigit(this ReadOnlySpan<char> s)
    {
        foreach (var c in s)
            if (char.IsDigit(c)) return true; // Unicode-десятичные цифры
        return false;
    }

    // Спец-символы: трактовка по Unicode категориям (Symbol или Punctuation)
    public static bool ContainsSpecialByUnicode(this ReadOnlySpan<char> s)
    {
        foreach (var c in s)
            if (char.IsSymbol(c) || char.IsPunctuation(c)) return true;
        return false;
    }
    
    public static bool ContainsDigitOrSpecialByUnicode(this ReadOnlySpan<char> s)
    {
        foreach (var c in s)
            if (char.IsDigit(c) || char.IsSymbol(c) || char.IsPunctuation(c)) return true;
        return false;
    }
}