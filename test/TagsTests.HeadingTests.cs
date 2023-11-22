
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    [TestClass]
    public class HeadingTests
    {
        private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldAllowToDisableUppercasedHeadings()
        {
            const string html = @"
        <h1>Heading 1</h1>
        <h2>heading 2</h2>
        <h3>heading 3</h3>
        <h4>heading 4</h4>
        <h5>heading 5</h5>
        <h6>heading 6</h6>
      ";
            const string expected = "Heading 1\n\n\nheading 2\n\n\nheading 3\n\nheading 4\n\nheading 5\n\nheading 6";
            htmlToText(html).ShouldBe(expected.ToUpper());
            var options = new HtmlToTextOptions();
            options.H1.options.uppercase = false;
            options.H2.options.uppercase = false;
            options.H3.options.uppercase = false;
            options.H4.options.uppercase = false;
            options.H5.options.uppercase = false;
            options.H6.options.uppercase = false;
            htmlToText(html, options).ShouldBe(expected);
        }
    }
}