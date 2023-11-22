
namespace UnDotNet.HtmlToText.Tests;

[TestClass]
public class HtmlToTextUtilsTests
{

    [TestMethod]
    [DataRow("Test1@", "\\u0054\\u0065\\u0073\\u0074\\u0031\\u0040")]
    [DataRow("Abc123[]!`", "\\u0041\\u0062\\u0063\\u0031\\u0032\\u0033\\u005b\\u005d\\u0021\\u0060")]
    [DataRow(" \t", @"\u0020\u0009")]
    [DataRow(" \t\r\n\f\u200b", @"\u0020\u0009\u000d\u000a\u000c\u200b")]
    public void CharactersToCodesWorks(string input, string expected)
    {
        var output = WhitespaceProcessor.CharactersToCodes(input);
        output.ShouldBe(expected);
    }
    
}