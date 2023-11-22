
namespace UnDotNet.HtmlToText.Tests;

[TestClass]
public class CustomSelectors
{
    private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
        new HtmlToTextConverter().Convert(html, options, metadata);

    [TestMethod]
    public void ShouldMergeEntriesWithTheSameSelector()
    {
        // this test isn't really applicable in DotNet, but I included it for completeness
        var html = "<foo></foo><foo></foo><foo></foo>";
        var expected = "----------\n\n\n\n----------\n\n\n\n----------";
        var options = new HtmlToTextOptions();
        options.Selectors.Add(new Selector() {Identifier = "foo", Format = "somethingElse"});
        options.Selector("foo").Options.Length = 20;
        options.Selector("foo").Options.LeadingLineBreaks = 4;
        options.Selector("foo").Options.TrailingLineBreaks = 4;
        options.Selector("foo").Options.Length = 10;
        options.Selector("foo").Format = "horizontalLine";
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldPickTheMostSpecificSelector()
    {
        var html = "<hr/><hr class=\"foo\" id=\"bar\"/>";
        var expected = "---\n\n-----";
        var options = new HtmlToTextOptions()
        {
            Selectors = new List<Selector>()
            {
                new () { Identifier = "hr", Format = "horizontalLine", Options = new() {Length = 3}},
                new () { Identifier = "hr#bar", Format = "horizontalLine", Options = new() {Length = 5}},
                new () { Identifier = "hr#bar", Format = "horizontalLine", Options = new() {Length = 7}},
            }
        };
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldPickTheLastSelectorOfEqualSpecificity()
    {
        // rather than picking the last selector, we follow CSS rules which follow order in the class
        var html = "<hr class=\"bar baz\"/><hr class=\"foo bar\"/><hr class=\"foo baz\"/>";
        var expected = "---\n\n-------\n\n-------";
        var options = new HtmlToTextOptions()
        {
            Selectors = new List<Selector>()
            {
                new () { Identifier = "hr.foo", Format = "horizontalLine", Options = new() {Length = 7}},
                new () { Identifier = "hr.baz", Format = "horizontalLine", Options = new() {Length = 3}},
                new () { Identifier = "hr.bar", Format = "horizontalLine", Options = new() {Length = 5}},
            }
        };
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldAllowEscapeSequencesInSelectors()
    {
        var html = "<hr id=\"sceneI_3.1\"/><hr class=\"---\"/>";
        var expected = "---[ cut ]---\n\n---[ cut ]---";
        var options = new HtmlToTextOptions();
        options.Selectors.Add(new Selector() {Identifier = "#sceneI_3\\.1", Format = "blockString", Options = new () {StringLiteral = "---[ cut ]---"} });
        options.Selectors.Add(new Selector() {Identifier = ".\\2d -\\-", Format = "blockString", Options = new () {StringLiteral = "---[ cut ]---"} });
        HtmlToText(html, options).ShouldBe(expected);
    }
    
}