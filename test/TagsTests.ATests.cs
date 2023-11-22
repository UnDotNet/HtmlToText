
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    
    [TestClass]
    public class ATests
    {

        private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);
        
        
        [TestMethod]
        public void ShouldDecodeHtmlAttributeEntitiesFromHref()
        {
            string html = "<a href=\"/foo?a&#x3D;b\">test</a>";
            string expected = "test [/foo?a=b]";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotInsertNullBytes()
        {
            string html = "<a href=\"some-url?a=b&amp;b=c\">Testing &amp; Done</a>";
            string expected = "Testing & Done [some-url?a=b&b=c]";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldUpdateRelativelySourcedLinksWithBaseUrl()
        {
            string html = "<a href=\"/test.html\">test</a>";
            var options = new HtmlToTextOptions();
            options.A.options.baseUrl = "https://example.com";
            string expected = "test [https://example.com/test.html]";
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldStripMailtoFromEmailLinks()
        {
            string html = "<a href=\"mailto:foo@example.com\">email me</a>";
            string expected = "email me [foo@example.com]";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnLinkWithBracketsByDefault()
        {
            string html = "<a href=\"http://my.link\">test</a>";
            string expected = "test [http://my.link]";
            htmlToText(html).ShouldBe(expected);
        }
    
        [TestMethod]
        public void ShouldReturnLinkWithoutBracketsIfLinkBracketsIsSetToFalse()
        {
            string html = "<a href=\"http://my.link\">test</a>";
            string expected = "test http://my.link";
            var options = new HtmlToTextOptions();
            options.A.options.linkBrackets = null;
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnLinkWithoutBracketsIfLinkBracketsIsSetToEmptyStrings()
        {
            string html = "<a href=\"http://my.link\">test</a>";
            string expected = "test http://my.link";
            var options = new HtmlToTextOptions();
            options.A.options.linkBrackets!.left = "";
            options.A.options.linkBrackets!.right = "";
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnLinkWithCustomBrackets()
        {
            string html = "<a href=\"http://my.link\">test</a>";
            string expected = "test ===> http://my.link <===";
            var options = new HtmlToTextOptions();
            options.A.options.linkBrackets!.left = "===> ";
            options.A.options.linkBrackets!.right = " <===";
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotReturnLinkForAnchorIfNoAnchorUrlIsSetToTrue()
        {
            string html = "<a href=\"#link\">test</a>";
            var options = new HtmlToTextOptions();
            options.A.options.noAnchorUrl = true;
            htmlToText(html, options).ShouldBe("test");
        }

        [TestMethod]
        public void ShouldReturnLinkForAnchorIfNoAnchorUrlIsSetToFalse()
        {
            string html = "<a href=\"#link\">test</a>";
            string expected = "test [#link]";
            var options = new HtmlToTextOptions();
            options.A.options.noAnchorUrl = false;
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotUppercaseLinksInsideHeadings()
        {
            string html = /*html*/"<h1><a href=\"http://example.com\">Heading</a></h1>";
            string expected = "HEADING [http://example.com]";
            htmlToText(html).ShouldBe(expected);
        }
    
        [TestMethod]
        public void ShouldNotUppercaseLinksInsideTableHeaderCells()
        {
            string html = /*html*/"""
                                    <table>
                                      <tr>
                                        <th>Header cell 1</th>
                                        <th><a href="http://example.com">Header cell 2</a></th>
                                        <td><a href="http://example.com">Regular cell</a></td>
                                      </tr>
                                    </table>
                                  """;
            string expected = "HEADER CELL 1   HEADER CELL 2 [http://example.com]   Regular cell [http://example.com]";
            var options = new HtmlToTextOptions();
            options.Table.format = "dataTable";
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRewriteLinkHrefPathWithProvidedMetadata()
        {
            string html = "<a href=\"/test.html\">test</a>";
            var options = new HtmlToTextOptions();
            options.A.options.baseUrl = "https://example.com";
            options.A.options.pathRewrite = (path, meta, elem) => meta is null ? null : meta["path"] + path;
            string expected = "test [https://example.com/foo/bar/test.html]";
            htmlToText(html, options, new () { {"path", "/foo/bar" }}).ShouldBe(expected);
        }        
    }
}