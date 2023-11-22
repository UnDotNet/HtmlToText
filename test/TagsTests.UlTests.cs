// ReSharper disable StringLiteralTypo

namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{

    [TestClass]
    public class UlTests
    {
    
        private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldHandleEmptyUnorderedLists()
        {
            var html = "<ul></ul>";
            HtmlToText(html).ShouldBe("");
        }

        [TestMethod]
        public void ShouldHandleUnorderedListWithMultipleElements()
        {
            var html = "<ul><li>foo</li><li>bar</li></ul>";
            var expected = " * foo\n * bar";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleUnorderedListPrefixOption()
        {
            var html = "<ul><li>foo</li><li>bar</li></ul>";
            var options = new HtmlToTextOptions();
            options.Ul.Options.ItemPrefix = " test ";
            var expected = " test foo\n test bar";
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleNestedUlCorrectly()
        {
            var html = @"<ul><li>foo<ul><li>bar<ul><li>baz.1</li><li>baz.2</li></ul></li></ul></li></ul>";
            var expected = " * foo\n   * bar\n     * baz.1\n     * baz.2";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleLongNestedUlCorrectly()
        {
            var html = @"<ul>
          <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>
          <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>
          <li>Inner:
            <ul>
              <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>
              <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>
            </ul>
          </li>
        </ul>";
            var expected =
                " * At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g\n" +
                "   u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.\n" +
                " * At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g\n" +
                "   u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.\n" +
                " * Inner:\n" +
                "   * At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d\n" +
                "     g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.\n" +
                "   * At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d\n" +
                "     g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
            HtmlToText(html).ShouldBe(expected);
        }
    }
}