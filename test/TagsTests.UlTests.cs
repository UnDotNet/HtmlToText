// ReSharper disable StringLiteralTypo

namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{

    [TestClass]
    public class UlTests
    {
    
        private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldHandleEmptyUnorderedLists()
        {
            string html = "<ul></ul>";
            htmlToText(html).ShouldBe("");
        }

        [TestMethod]
        public void ShouldHandleUnorderedListWithMultipleElements()
        {
            string html = "<ul><li>foo</li><li>bar</li></ul>";
            string expected = " * foo\n * bar";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleUnorderedListPrefixOption()
        {
            string html = "<ul><li>foo</li><li>bar</li></ul>";
            var options = new HtmlToTextOptions();
            options.Ul.options.itemPrefix = " test ";
            string expected = " test foo\n test bar";
            htmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleNestedUlCorrectly()
        {
            string html = @"<ul><li>foo<ul><li>bar<ul><li>baz.1</li><li>baz.2</li></ul></li></ul></li></ul>";
            string expected = " * foo\n   * bar\n     * baz.1\n     * baz.2";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleLongNestedUlCorrectly()
        {
            string html = @"<ul>
          <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>
          <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>
          <li>Inner:
            <ul>
              <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>
              <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>
            </ul>
          </li>
        </ul>";
            string expected =
                " * At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g\n" +
                "   u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.\n" +
                " * At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g\n" +
                "   u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.\n" +
                " * Inner:\n" +
                "   * At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d\n" +
                "     g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.\n" +
                "   * At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d\n" +
                "     g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
            htmlToText(html).ShouldBe(expected);
        }
    }
}