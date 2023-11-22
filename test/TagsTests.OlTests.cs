// ReSharper disable StringLiteralTypo

namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{

    [TestClass]
    public class OlTests
    {

        private string htmlToText(string? html, HtmlToTextOptions? options = null,
            Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldHandleEmptyOrderedLists()
        {
            var html = "<ol></ol>";
            htmlToText(html).ShouldBe("");
        }

        [TestMethod]
        public void ShouldHandleAnOrderedListWithMultipleElements()
        {
            var html = "<ol><li>foo</li><li>bar</li></ol>";
            var expected = " 1. foo\n 2. bar";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportTheOrderedListType1Attribute()
        {
            var html = "<ol type=\"1\"><li>foo</li><li>bar</li></ol>";
            var expected = " 1. foo\n 2. bar";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldFallbackToType1BehaviorIfTypeAttributeIsInvalid()
        {
            var html = "<ol type=\"whatever\"><li>foo</li><li>bar</li></ol>";
            var expected = " 1. foo\n 2. bar";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportTheOrderedListTypeaAttribute()
        {
            var html = "<ol type=\"a\"><li>foo</li><li>bar</li></ol>";
            var expected = " a. foo\n b. bar";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportTheOrderedListTypeAAttribute()
        {
            var html = "<ol type=\"A\"><li>foo</li><li>bar</li></ol>";
            var expected = " A. foo\n B. bar";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportTheOrderedListTypeiAttribute()
        {
            var html1 = "<ol type=\"i\"><li>foo</li><li>bar</li></ol>";
            var html2 = "<ol start=\"8\" type=\"i\"><li>foo</li><li>bar</li></ol>";
            htmlToText(html1).ShouldBe(" i.  foo\n ii. bar");
            htmlToText(html2).ShouldBe(" viii. foo\n ix.   bar");
        }

        [TestMethod]
        public void ShouldSupportTheOrderedListTypeIAttribute()
        {
            var html1 = "<ol type=\"I\"><li>foo</li><li>bar</li></ol>";
            var html2 = "<ol start=\"8\" type=\"I\"><li>foo</li><li>bar</li></ol>";
            htmlToText(html1).ShouldBe(" I.  foo\n II. bar");
            htmlToText(html2).ShouldBe(" VIII. foo\n IX.   bar");
        }

        [TestMethod]
        public void ShouldSupportTheOrderedListStartAttribute()
        {
            var html = "<ol start=\"100\"><li>foo</li><li>bar</li></ol>";
            var expected = " 100. foo\n 101. bar";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleNestedOlCorrectly()
        {
            var html = "<ol><li>foo<ol><li>bar<ol><li>baz</li><li>baz</li></ol></li></ol></li></ol>";
            var expected = " 1. foo\n    1. bar\n       1. baz\n       2. baz";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleLongNestedOlCorrectly()
        {
            var html = /*html*/"<ol>\n" +
                               "  <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>\n" +
                               "  <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>\n" +
                               "  <li>Inner:\n" +
                               "    <ol>\n" +
                               "      <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>\n" +
                               "      <li>At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.</li>\n" +
                               "    </ol>\n" +
                               "  </li>\n" +
                               "</ol>";
            var expected =
                " 1. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d\n" +
                "    g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.\n" +
                " 2. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s d\n" +
                "    g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit amet.\n" +
                " 3. Inner:\n" +
                "    1. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s\n" +
                "       d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit\n" +
                "       amet.\n" +
                "    2. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita k a s\n" +
                "       d g u b e r g r e n, no sea takimata sanctus est Lorem ipsum dolor sit\n" +
                "       amet.";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportTheOrderedListTypeaAttributePast26Characters()
        {
            var html1 = "<ol start=\"26\" type=\"a\"><li>foo</li><li>bar</li></ol>";
            var html2 = "<ol start=\"702\" type=\"a\"><li>foo</li><li>bar</li></ol>";
            htmlToText(html1).ShouldBe(" z.  foo\n aa. bar");
            htmlToText(html2).ShouldBe(" zz.  foo\n aaa. bar");
        }

        [TestMethod]
        public void ShouldSupportTheOrderedListTypeAAttributePast26Characters()
        {
            var html1 = "<ol start=\"26\" type=\"A\"><li>foo</li><li>bar</li></ol>";
            var html2 = "<ol start=\"702\" type=\"A\"><li>foo</li><li>bar</li></ol>";
            htmlToText(html1).ShouldBe(" Z.  foo\n AA. bar");
            htmlToText(html2).ShouldBe(" ZZ.  foo\n AAA. bar");
        }

        [TestMethod]
        public void ShouldNotWrapLiWhenWordwrapIsDisabled()
        {
            var html = /*html*/"Good morning Jacob,\n" +
                               "  <p>Lorem ipsum dolor sit amet</p>\n" +
                               "  <p><strong>Lorem ipsum dolor sit amet.</strong></p>\n" +
                               "  <ul>\n" +
                               "    <li>run in the park <span style=\"color:#888888;\">(in progress)</span></li>\n" +
                               "  </ul>\n" +
                               "";
            var expected =
                "Good morning Jacob,\n\nLorem ipsum dolor sit amet\n\nLorem ipsum dolor sit amet.\n\n * run in the park (in progress)";
            htmlToText(html, new () { wordwrap = null }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleNonLiElementsInsideAListGracefully()
        {
            var html = /*html*/"\n" +
                               "  <ul>\n" +
                               "    <li>list item</li>\n" +
                               "    plain text\n" +
                               "    <li>list item</li>\n" +
                               "    <div>div</div>\n" +
                               "    <li>list item</li>\n" +
                               "    <p>paragraph</p>\n" +
                               "    <li>list item</li>\n" +
                               "  </ul>\n" +
                               "";
            var expected =
                " * list item\n   plain text\n * list item\n   div\n * list item\n\n   paragraph\n\n * list item";
            htmlToText(html).ShouldBe(expected);
        }
    }
    
}