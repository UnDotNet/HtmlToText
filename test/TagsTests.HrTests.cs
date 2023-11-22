
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    [TestClass]
    public class HrTests
    {

        private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);
        
        [TestMethod]
        public void ShouldOutputHorizontalLineOfDefaultLength()
        {
            var html = "<div>foo</div><hr/><div>bar</div>";
            var expected = "foo\n\n--------------------------------------------------------------------------------\n\nbar";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldOutputHorizontalLineOfSpecificLength()
        {
            var html = "<div>foo</div><hr/><div>bar</div>";
            var expected = "foo\n\n------------------------------\n\nbar";
            var options = new HtmlToTextOptions();
            options.Hr.Options.Length = 30;
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldOutputHorizontalLineOfLength40WhenWordwrapIsDisabled()
        {
            var html = "<div>foo</div><hr/><div>bar</div>";
            var expected = "foo\n\n----------------------------------------\n\nbar";
            HtmlToText(html, new () { Wordwrap = 0 }).ShouldBe(expected);
        }
    }
}