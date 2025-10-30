namespace cache_me_if_you_can.Services;

public static class ComandanteParser
{
    public static MultiSpan Parse(ReadOnlySpan<char> span)
    {
        var result = new MultiSpan();
        if (span.IsEmpty)
            return result;

        // Разделители: пробел
        ReadOnlySpan<char> seps = [' ']; 
        // i — текущая позиция в исходном span
        var i = 0;
        var part = 0;

        // Пропускаем ведущие разделители
        while (i < span.Length && seps.IndexOf(span[i]) >= 0) i++;

        while (i < span.Length && part < MultiSpan.Length)
        {
            var start = i;

            // Найти ближайший разделитель среди заданных
            var rel = span.Slice(i).IndexOfAny(seps);
            if (rel < 0)
            {
                // Разделителей больше нет — берём остаток
                result[part] = span.Slice(start);
                break;
            }

            // Срез до разделителя
            var end = i + rel;
            result[part++] = span.Slice(start, end - start);

            // Сдвигаем i сразу за последовательность разделителей
            i = end + 1;
            while (i < span.Length && seps.IndexOf(span[i]) >= 0) i++;
        }

        if (result[1].IsEmpty || result[0].ContainsDigitOrSpecialByUnicode() ) 
            return new MultiSpan();
        
        return result;
    }
    
    public static MultiSpan Parse(ReadOnlyMemory<char> memoryBlock)
    {
        return Parse(memoryBlock.Span);
    }
}
