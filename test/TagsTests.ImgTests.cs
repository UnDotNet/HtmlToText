
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    [TestClass]
    public class ImgTests
    {
    
        private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldReplaceEntitiesInsideAltAttributesOfImages()
        {
            string html = "<img src=\"test.png\" alt=\"&quot;Awesome&quot;\">";
            string expected = "\"Awesome\" [test.png]";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldUpdateRelativelySourcedImagesWithBaseUrl()
        {
            string html = "<img src=\"/test.png\">";
            var options = new HtmlToTextOptions();
            options.Img.options.baseUrl = "https://example.com";
            string expected = "[https://example.com/test.png]";
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnImageLinkWithoutBracketsIfLinkBracketsIsSetToFalse()
        {
            string html = "<img src=\"test.png\" alt=\"Awesome\">";
            string expected = "Awesome test.png";
            var options = new HtmlToTextOptions();
            options.Img.options.linkBrackets = null;
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnImageLinkWithoutBracketsIfLinkBracketsIsSetToEmptyArray()
        {
            string html = "<img src=\"test.png\" alt=\"Awesome\">";
            string expected = "Awesome test.png";
            var options = new HtmlToTextOptions();
            options.Img.options.linkBrackets!.left = "";
            options.Img.options.linkBrackets!.right = "";
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnImageLinkWithCustomBrackets()
        {
            string html = "<img src=\"test.png\" alt=\"Awesome\">";
            string expected = "Awesome ===> test.png <===";
            var options = new HtmlToTextOptions();
            options.Img.options.linkBrackets!.left = "===> ";
            options.Img.options.linkBrackets!.right = " <===";
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRewriteImageSourcePathWithProvidedMetadata()
        {
            string html = "<img src=\"pictures/test.png\">";
            string expected = "[assets/test.png]";
            var options = new HtmlToTextOptions();
            options.Img.options.pathRewrite = (path, meta, _) => path.Replace("pictures/", meta?["assetsPath"]);
            htmlToText(html, options, new() {{"assetsPath", "assets/"}}).ShouldBe(expected);
        }
    }
}