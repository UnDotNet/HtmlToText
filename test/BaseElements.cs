// ReSharper disable StringLiteralTypo
// ReSharper disable UseObjectOrCollectionInitializer

namespace UnDotNet.HtmlToText.Tests;

[TestClass]
public class BaseElementTests
{
    private string baseFilePath = "testassets";
    
    private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
        new HtmlToTextConverter().Convert(html, options, metadata);

    [TestMethod]
    public void ShouldRetrieveAndConvertEntireDocumentByDefault()
    {
        var html = File.ReadAllText($"{baseFilePath}/test.html");
        var expected = File.ReadAllText($"{baseFilePath}/test.txt");
        var options = new HtmlToTextOptions();
        options.selectors.Add(new Selector() {selector = "table#invoice", format = "dataTable"});
        options.selectors.Add(new Selector() {selector = "table.address", format = "dataTable"});
        htmlToText(html, options).ShouldBe(expected);
    }

    [TestMethod]
    public void ShouldOnlyRetrieveAndConvertContentUnderSpecifiedBaseElementIfFound()
    {
        var html = File.ReadAllText($"{baseFilePath}/test.html");
        var expected = File.ReadAllText($"{baseFilePath}/test-address.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.selectors = new() { "table.address" };
        options.selectors.Add(new Selector() {selector = "table.address", format = "dataTable"});
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldNotRepeatSameBaseElement()
    {
        var html = File.ReadAllText($"{baseFilePath}/test.html");
        var expected = File.ReadAllText($"{baseFilePath}/test-address.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.selectors = new() { "table.address", "table.address" };
        options.selectors.Add(new Selector() {selector = "table.address", format = "dataTable"});
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldRetrieveBaseElementsInOrderByOccurrence()
    {
        var html = File.ReadAllText($"{baseFilePath}/test.html");
        var expected = File.ReadAllText($"{baseFilePath}/test-orderby-occurrence.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.selectors = new() { "p.normal-space.small", "table.address" };
        options.BaseElements.orderBy = "occurrence";
        options.selectors.Add(new Selector() {selector = "table.address", format = "dataTable"});
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldRetrieveBaseElementsInOrderBySelectors()
    {
        var html = File.ReadAllText($"{baseFilePath}/test.html");
        var expected = File.ReadAllText($"{baseFilePath}/test-orderby-selectors.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.selectors = new() { "p.normal-space.small", "table.address" };
        options.BaseElements.orderBy = "selectors";
        options.selectors.Add(new Selector() {selector = "table.address", format = "dataTable"});
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldRetrieveAllDifferentBaseElementsMatchedSameSelector()
    {
        var html = File.ReadAllText($"{baseFilePath}/test.html");
        var expected = File.ReadAllText($"{baseFilePath}/test-multiple-elements.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.selectors = new() { "p.normal-space" };
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldRespectMaxBaseElementsLimit()
    {
        var html = @"<!DOCTYPE html><html><head></head><body><p>a</p><div>div</div><p>b</p><p>c</p><p>d</p><p>e</p><p>f</p><p>g</p><p>h</p><p>i</p><p>j</p></body></html>";
        var expected = "a\n\ndiv\n\nb";
        var options = new HtmlToTextOptions();
        options.BaseElements.selectors = new() { "p", "div" };
        options.BaseElements.orderBy = "occurrence";
        options.limits.maxBaseElements = 3;
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldRetrieveAndConvertEntireDocumentByDefaultIfNoBaseElementFound()
    {
        var html = File.ReadAllText($"{baseFilePath}/test.html");
        var expected = File.ReadAllText($"{baseFilePath}/test.txt");
        var options = new HtmlToTextOptions();
        options.BaseElements.selectors = new() { "table.nothere" };
        options.selectors.Add(new Selector() {selector = "table#invoice", format = "dataTable"});
        options.selectors.Add(new Selector() {selector = "table.address", format = "dataTable"});
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldReturnNullIfBaseElementNotFoundAndNotReturningDomByDefault()
    {
        var html = File.ReadAllText($"{baseFilePath}/test.html");
        var expected = "";
        var options = new HtmlToTextOptions();
        options.BaseElements.selectors = new() { "table.nothere" };
        options.BaseElements.returnDomByDefault = false;
        htmlToText(html, options).ShouldBe(expected);
    }
}