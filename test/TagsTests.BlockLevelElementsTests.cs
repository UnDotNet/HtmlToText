
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    [TestClass]
    public class BlockLevelElementsTests
    {
        
        private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);
        
        [TestMethod]
        public void ShouldRenderCommonBlockLevelElementsOnSeparateLinesWithDefaultLineBreaksNumber()
        {
            string html =
                "a<article>article</article>b<aside>aside</aside>c<div>div</div>d<footer>footer</footer>" +
                "e<form>form</form>f<header>header</header>g<main>main</main>h<nav>nav</nav>i<section>section</section>j";
            string expected = "a\narticle\nb\naside\nc\ndiv\nd\nfooter\ne\nform\nf\nheader\ng\nmain\nh\nnav\ni\nsection\nj";
            htmlToText(html, null).ShouldBe(expected);
        }
    }
}