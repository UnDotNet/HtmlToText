
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    [TestClass]
    public class ImgTests
    {
    
        private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldReplaceEntitiesInsideAltAttributesOfImages()
        {
            var html = "<img src=\"test.png\" alt=\"&quot;Awesome&quot;\">";
            var expected = "\"Awesome\" [test.png]";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldUpdateRelativelySourcedImagesWithBaseUrl()
        {
            var html = "<img src=\"/test.png\">";
            var options = new HtmlToTextOptions();
            options.Img.Options.BaseUrl = "https://example.com";
            var expected = "[https://example.com/test.png]";
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnImageLinkWithoutBracketsIfLinkBracketsIsSetToFalse()
        {
            var html = "<img src=\"test.png\" alt=\"Awesome\">";
            var expected = "Awesome test.png";
            var options = new HtmlToTextOptions();
            options.Img.Options.LinkBrackets = null;
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnImageLinkWithoutBracketsIfLinkBracketsIsSetToEmptyArray()
        {
            var html = "<img src=\"test.png\" alt=\"Awesome\">";
            var expected = "Awesome test.png";
            var options = new HtmlToTextOptions();
            options.Img.Options.LinkBrackets!.Left = "";
            options.Img.Options.LinkBrackets!.Right = "";
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldReturnImageLinkWithCustomBrackets()
        {
            var html = "<img src=\"test.png\" alt=\"Awesome\">";
            var expected = "Awesome ===> test.png <===";
            var options = new HtmlToTextOptions();
            options.Img.Options.LinkBrackets!.Left = "===> ";
            options.Img.Options.LinkBrackets!.Right = " <===";
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRewriteImageSourcePathWithProvidedMetadata()
        {
            var html = "<img src=\"pictures/test.png\">";
            var expected = "[assets/test.png]";
            var options = new HtmlToTextOptions();
            options.Img.Options.PathRewrite = (path, meta, _) => path.Replace("pictures/", meta?["assetsPath"]);
            HtmlToText(html, options, new() {{"assetsPath", "assets/"}}).ShouldBe(expected);
        }
    }
}