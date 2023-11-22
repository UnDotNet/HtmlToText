
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    [TestClass]
    public class HrTests
    {

        private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);
        
        [TestMethod]
        public void ShouldOutputHorizontalLineOfDefaultLength()
        {
            string html = "<div>foo</div><hr/><div>bar</div>";
            string expected = "foo\n\n--------------------------------------------------------------------------------\n\nbar";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldOutputHorizontalLineOfSpecificLength()
        {
            string html = "<div>foo</div><hr/><div>bar</div>";
            string expected = "foo\n\n------------------------------\n\nbar";
            var options = new HtmlToTextOptions();
            options.Hr.options.length = 30;
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldOutputHorizontalLineOfLength40WhenWordwrapIsDisabled()
        {
            string html = "<div>foo</div><hr/><div>bar</div>";
            string expected = "foo\n\n----------------------------------------\n\nbar";
            htmlToText(html, new () { wordwrap = 0 }).ShouldBe(expected);
        }
    }
}