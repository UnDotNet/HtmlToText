// ReSharper disable StringLiteralTypo
// ReSharper disable UseObjectOrCollectionInitializer

namespace UnDotNet.HtmlToText.Tests;

[TestClass]
public class BaseElementTests
{
    private const string BaseFilePath = "testassets";

    private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
        new HtmlToTextConverter().Convert(html, options, metadata);

    [TestMethod]
    public void ShouldRetrieveAndConvertEntireDocumentByDefault()
    {
        var html = File.ReadAllText($"{BaseFilePath}/test.html");
        var expected = File.ReadAllText($"{BaseFilePath}/test.txt");
        var options = new HtmlToTextOptions();
        options.Selectors.Add(new Selector() {Identifier = "table#invoice", Format = "dataTable"});
        options.Selectors.Add(new Selector() {Identifier = "table.address", Format = "dataTable"});
        HtmlToText(html, options).ShouldBe(expected);
    }

    [TestMethod]
    public void ShouldOnlyRetrieveAndConvertContentUnderSpecifiedBaseElementIfFound()
    {
        var html = File.ReadAllText($"{BaseFilePath}/test.html");
        var expected = File.ReadAllText($"{BaseFilePath}/test-address.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.Selectors = new() { "table.address" };
        options.Selectors.Add(new Selector() {Identifier = "table.address", Format = "dataTable"});
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldNotRepeatSameBaseElement()
    {
        var html = File.ReadAllText($"{BaseFilePath}/test.html");
        var expected = File.ReadAllText($"{BaseFilePath}/test-address.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.Selectors = new() { "table.address", "table.address" };
        options.Selectors.Add(new Selector() {Identifier = "table.address", Format = "dataTable"});
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldRetrieveBaseElementsInOrderByOccurrence()
    {
        var html = File.ReadAllText($"{BaseFilePath}/test.html");
        var expected = File.ReadAllText($"{BaseFilePath}/test-orderby-occurrence.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.Selectors = new() { "p.normal-space.small", "table.address" };
        options.BaseElements.OrderBy = "occurrence";
        options.Selectors.Add(new Selector() {Identifier = "table.address", Format = "dataTable"});
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldRetrieveBaseElementsInOrderBySelectors()
    {
        var html = File.ReadAllText($"{BaseFilePath}/test.html");
        var expected = File.ReadAllText($"{BaseFilePath}/test-orderby-selectors.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.Selectors = new() { "p.normal-space.small", "table.address" };
        options.BaseElements.OrderBy = "selectors";
        options.Selectors.Add(new Selector() {Identifier = "table.address", Format = "dataTable"});
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldRetrieveAllDifferentBaseElementsMatchedSameSelector()
    {
        var html = File.ReadAllText($"{BaseFilePath}/test.html");
        var expected = File.ReadAllText($"{BaseFilePath}/test-multiple-elements.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.Selectors = new() { "p.normal-space" };
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldRespectMaxBaseElementsLimit()
    {
        var html = @"<!DOCTYPE html><html><head></head><body><p>a</p><div>div</div><p>b</p><p>c</p><p>d</p><p>e</p><p>f</p><p>g</p><p>h</p><p>i</p><p>j</p></body></html>";
        var expected = "a\n\ndiv\n\nb";
        var options = new HtmlToTextOptions();
        options.BaseElements.Selectors = new() { "p", "div" };
        options.BaseElements.OrderBy = "occurrence";
        options.Limits.MaxBaseElements = 3;
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldRetrieveAndConvertEntireDocumentByDefaultIfNoBaseElementFound()
    {
        var html = File.ReadAllText($"{BaseFilePath}/test.html");
        var expected = File.ReadAllText($"{BaseFilePath}/test.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.Selectors = new() { "table.nothere" };
        options.Selectors.Add(new Selector() {Identifier = "table#invoice", Format = "dataTable"});
        options.Selectors.Add(new Selector() {Identifier = "table.address", Format = "dataTable"});
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldReturnNullIfBaseElementNotFoundAndNotReturningDomByDefault()
    {
        var html = File.ReadAllText($"{BaseFilePath}/test.html");
        var expected = "";
        var options = new HtmlToTextOptions();
        options.BaseElements.Selectors = new() { "table.nothere" };
        options.BaseElements.ReturnDomByDefault = false;
        HtmlToText(html, options).ShouldBe(expected);
    }
}