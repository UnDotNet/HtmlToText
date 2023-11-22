
// ReSharper disable UnusedParameter.Local
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    
    [TestClass]
    public class ATests
    {

        private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);
        
        
        [TestMethod]
        public void ShouldDecodeHtmlAttributeEntitiesFromHref()
        {
            var html = "<a href=\"/foo?a&#x3D;b\">test</a>";
            var expected = "test [/foo?a=b]";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotInsertNullBytes()
        {
            var html = "<a href=\"some-url?a=b&amp;b=c\">Testing &amp; Done</a>";
            var expected = "Testing & Done [some-url?a=b&b=c]";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldUpdateRelativelySourcedLinksWithBaseUrl()
        {
            var html = "<a href=\"/test.html\">test</a>";
            var options = new HtmlToTextOptions();
            options.A.Options.BaseUrl = "https://example.com";
            var expected = "test [https://example.com/test.html]";
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldStripMailtoFromEmailLinks()
        {
            var html = "<a href=\"mailto:foo@example.com\">email me</a>";
            var expected = "email me [foo@example.com]";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnLinkWithBracketsByDefault()
        {
            var html = "<a href=\"https://my.link\">test</a>";
            var expected = "test [https://my.link]";
            HtmlToText(html).ShouldBe(expected);
        }
    
        [TestMethod]
        public void ShouldReturnLinkWithoutBracketsIfLinkBracketsIsSetToFalse()
        {
            var html = "<a href=\"https://my.link\">test</a>";
            var expected = "test https://my.link";
            var options = new HtmlToTextOptions();
            options.A.Options.LinkBrackets = null;
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnLinkWithoutBracketsIfLinkBracketsIsSetToEmptyStrings()
        {
            var html = "<a href=\"https://my.link\">test</a>";
            var expected = "test https://my.link";
            var options = new HtmlToTextOptions();
            options.A.Options.LinkBrackets!.Left = "";
            options.A.Options.LinkBrackets!.Right = "";
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnLinkWithCustomBrackets()
        {
            var html = "<a href=\"https://my.link\">test</a>";
            var expected = "test ===> https://my.link <===";
            var options = new HtmlToTextOptions();
            options.A.Options.LinkBrackets!.Left = "===> ";
            options.A.Options.LinkBrackets!.Right = " <===";
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotReturnLinkForAnchorIfNoAnchorUrlIsSetToTrue()
        {
            var html = "<a href=\"#link\">test</a>";
            var options = new HtmlToTextOptions();
            options.A.Options.NoAnchorUrl = true;
            HtmlToText(html, options).ShouldBe("test");
        }

        [TestMethod]
        public void ShouldReturnLinkForAnchorIfNoAnchorUrlIsSetToFalse()
        {
            var html = "<a href=\"#link\">test</a>";
            var expected = "test [#link]";
            var options = new HtmlToTextOptions();
            options.A.Options.NoAnchorUrl = false;
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotUppercaseLinksInsideHeadings()
        {
            var html = /*html*/"<h1><a href=\"https://example.com\">Heading</a></h1>";
            var expected = "HEADING [https://example.com]";
            HtmlToText(html).ShouldBe(expected);
        }
    
        [TestMethod]
        public void ShouldNotUppercaseLinksInsideTableHeaderCells()
        {
            var html = /*html*/"""
                                 <table>
                                   <tr>
                                     <th>Header cell 1</th>
                                     <th><a href="https://example.com">Header cell 2</a></th>
                                     <td><a href="https://example.com">Regular cell</a></td>
                                   </tr>
                                 </table>
                               """;
            var expected = "HEADER CELL 1   HEADER CELL 2 [https://example.com]   Regular cell [https://example.com]";
            var options = new HtmlToTextOptions
            {
                Table =
                {
                    Format = "dataTable"
                }
            };
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRewriteLinkHrefPathWithProvidedMetadata()
        {
            var html = "<a href=\"/test.html\">test</a>";
            var expected = "test [https://example.com/foo/bar/test.html]";
            var options = new HtmlToTextOptions();
            options.A.Options.BaseUrl = "https://example.com";
            options.A.Options.PathRewrite = (path, meta, elem) => meta is null ? null : meta["path"] + path;
            HtmlToText(html, options, new Dictionary<string, string> { {"path", "/foo/bar" }}).ShouldBe(expected);
        }        
    }
}