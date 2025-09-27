namespace cache_me_if_you_can.Tests;

public class ComandanteParserTests
{
    [Theory]
    [InlineData("SET user:1 data")]
    [InlineData("         SET user:1 data")]
    [InlineData("SET user:1 data         ")]
    [InlineData("SET          user:1 data")]
    [InlineData("SET user:1          data")]
    public void ComandanteParser_ParseIgnoresExtraWhitespace_AllPartsAreNonEmpty(string testString)
    {
        var multispan = ComandanteParser.Parse(testString.AsSpan());

        var result = (multispan.Command.IsEmpty, multispan.Key.IsEmpty, multispan.Value.IsEmpty);
        var expected = (false, false, false);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("GET user:1")]
    [InlineData("           GET user:1")]
    [InlineData("GET            user:1")]
    [InlineData("GET user:1           ")]
    public void ComandanteParser_ParseIgnoresExtraWhitespace_CommandKeyAreNonEmpty(string testString)
    {
        var multispan = ComandanteParser.Parse(testString.AsSpan());

        var result = (multispan.Command.IsEmpty, multispan.Key.IsEmpty, multispan.Value.IsEmpty);
        var expected = (false, false, true);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("SET")]
    [InlineData("")]
    [InlineData("123 user:1 data")]
    [InlineData("!@# user:1 data")]
    public void ComandanteParser_ParseErrorData_AllPartsAreEmpty(string testString)
    {
        var multispan = ComandanteParser.Parse(testString.AsSpan());

        var result = (multispan.Command.IsEmpty, multispan.Key.IsEmpty, multispan.Value.IsEmpty);
        var expected = (true, true, true);
        Assert.Equal(expected, result);
    }
}