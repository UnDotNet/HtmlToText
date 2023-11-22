// ReSharper disable StringLiteralTypo
// ReSharper disable UseObjectOrCollectionInitializer

using System.Diagnostics;

namespace UnDotNet.HtmlToText.Tests;

[TestClass]
public class ConverterTests
{
    [TestClass]
    public class SmokeTests
    {
        
        private readonly HtmlToTextConverter defaultConvert = new();

        [TestMethod]
        public void ShouldReturnEmptyInputUnchanged()
        {
            defaultConvert.Convert("").ShouldBe("");
        }

        [TestMethod]
        public void ShouldReturnEmptyResultIfInputIsNull()
        {
            defaultConvert.Convert().ShouldBe("");
        }

        [TestMethod]
        public void ShouldReturnPlainTextUnchanged()
        {
            defaultConvert.Convert("Hello world!").ShouldBe("Hello world!");
        }
    }

    [TestClass]
    public class SkippedHtmlContent
    {
        private readonly HtmlToTextConverter defaultConvert = new();
        
        [TestMethod]
        public void ShouldIgnoreHtmlComments()
        {
            string html = @"<!--[^-]*-->
                        <!-- <h1>Hello World</h1> -->
                        text";
            defaultConvert.Convert(html).ShouldBe("text");
        }

        [TestMethod]
        public void ShouldIgnoreScripts()
        {
            string html = @"<script src=""javascript.js""></script>
                        <script>
                          console.log(""Hello World!"");
                        </script>
                        <script id=""data"" type=""application/json"">{'userId':1234,'userName':'John Doe','memberSince':'2000-01-01T00:00:00.000Z'}</script>
                        text";
            defaultConvert.Convert(html).ShouldBe("text");
        }

        [TestMethod]
        public void ShouldIgnoreStyles()
        {
            string html = @"<link href=""main.css"" rel=""stylesheet"">
                        <style type=""text/css"" media=""all and (max-width: 500px)"">
                          p { color: #26b72b; }
                        </style>
                        text";
            defaultConvert.Convert(html).ShouldBe("text");
        }

        [TestMethod]
        public void ShouldNotBreakAfterSpecialTagFollowedByEntity()
        {
            string html = @"<style>a{}</style>&apos;<br/><span>text</span>";
            defaultConvert.Convert(html).ShouldBe("'\ntext");
        }
    }

    [TestClass]
    public class WordWrapOption
    {
        private readonly HtmlToTextConverter defaultConvert = new();

        [TestMethod]
        public void ShouldWordWrapAt80CharactersByDefault()
        {
            string longStr = "111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888 999999999";
            defaultConvert.Convert(longStr).ShouldBe("111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888\n999999999");
        }

        [TestMethod]
        public void ShouldWordWrapAtGivenNumberOfCharacters()
        {
            string longStr = "111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888 999999999";
            defaultConvert.Convert(longStr, new HtmlToTextOptions { wordwrap = 20 }).ShouldBe("111111111 222222222\n333333333 444444444\n555555555 666666666\n777777777 888888888\n999999999");
            defaultConvert.Convert(longStr, new HtmlToTextOptions { wordwrap = 50 }).ShouldBe("111111111 222222222 333333333 444444444 555555555\n666666666 777777777 888888888 999999999");
            defaultConvert.Convert(longStr, new HtmlToTextOptions { wordwrap = 70 }).ShouldBe("111111111 222222222 333333333 444444444 555555555 666666666 777777777\n888888888 999999999");
        }

        [TestMethod]
        public void ShouldNotWordWrapWhenGivenNull()
        {
            string longStr = "111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888 999999999";
            defaultConvert.Convert(longStr, new HtmlToTextOptions { wordwrap = null }).ShouldBe(longStr);
        }

        [TestMethod]
        public void ShouldNotWordWrapWhenGivenFalse()
        {
            string longStr = "111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888 999999999";
            defaultConvert.Convert(longStr, new HtmlToTextOptions { wordwrap = 0 }).ShouldBe(longStr);
        }

        [TestMethod]
        public void ShouldNotExceedTheLineWidthWhenProcessingEmbeddedFormatTags()
        {
            string html = "<p><strong>This text isn't counted</strong> when calculating where to break a string for 80 character line lengths.</p>";
            string expected = "This text isn't counted when calculating where to break a string for 80\ncharacter line lengths.";
            defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWorkWithALongStringContainingLineFeeds()
        {
            string html = "<p>If a word with a line feed exists over the line feed boundary then\nyou\nmust\nrespect it.</p>";
            string expected = "If a word with a line feed exists over the line feed boundary then you must\nrespect it.";
            defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotWronglyTruncateLinesWhenProcessingEmbeddedFormatTags()
        {
            string html = "<p><strong>This text isn't counted</strong> when calculating where to break a string for 80 character line lengths.  However it can affect where the next line breaks and this could lead to having an early line break</p>";
            string expected = "This text isn't counted when calculating where to break a string for 80\ncharacter line lengths. However it can affect where the next line breaks and\nthis could lead to having an early line break";
            defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotExceedTheLineWidthWhenProcessingAnchorTags()
        {
            string html = "<p>We appreciate your business. And we hope you'll check out our <a href=\"http://example.com/\">new products</a>!</p>";
            string expected = "We appreciate your business. And we hope you'll check out our new products\n[http://example.com/]!";
            defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHonourLineFeedsFromALongWordAcrossTheWrapWhereTheLineFeedIsBeforeTheWrap()
        {
            string html = "<p>This string is meant to test if a string is split properly across a\nnewlineandlongword with following text.</p>";
            string expected = "This string is meant to test if a string is split properly across a\nnewlineandlongword with following text.";
            defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRemoveLineFeedsFromALongWordAcrossTheWrapWhereTheLineFeedIsAfterTheWrap()
        {
            string html = "<p>This string is meant to test if a string is split properly across anewlineandlong\nword with following text.</p>";
            string expected = "This string is meant to test if a string is split properly across\nanewlineandlong word with following text.";
            defaultConvert.Convert(html).ShouldBe(expected);
        }
    }

    [TestClass]
    public class PreserveNewlinesTests
    {
        private readonly HtmlToTextConverter defaultConvert = new();
        
        [TestMethod]
        public void ShouldNotPreserveNewlinesByDefault()
        {
            string html = "<p\n>One\nTwo\nThree</p>";
            string expected = "One Two Three";
            defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldPreserveNewlinesWhenProvidedWithTruthyValue()
        {
            string html = "<p\n>One\nTwo\nThree</p>";
            string expected = "One\nTwo\nThree";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldPreserveLineFeedsInALongWrappingStringContainingLineFeeds()
        {
            string html = "<p>If a word with a line feed exists over the line feed boundary then\nyou\nmust\nrespect it.</p>";
            string expected = "If a word with a line feed exists over the line feed boundary then\nyou\nmust\nrespect it.";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldPreserveLineFeedsInALongStringContainingLineFeedsAcrossTheWrap()
        {
            string html = "<p>If a word with a line feed exists over the line feed boundary then\nyou must respect it.</p>";
            string expected = "If a word with a line feed exists over the line feed boundary then\nyou must respect it.";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldPreserveLineFeedsInALongStringContainingLineFeedsAcrossTheWrapWithALineFeedBefore80Chars()
        {
            string html = "<p>This string is meant to test if a string is split properly across a\nnewlineandlongword with following text.</p>";
            string expected = "This string is meant to test if a string is split properly across a\nnewlineandlongword with following text.";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldPreserveLineFeedsInALongStringContainingLineFeedsAcrossTheWrapWithALineFeedAfter80Chars()
        {
            string html = "<p>This string is meant to test if a string is split properly across anewlineandlong\nword with following text.</p>";
            string expected = "This string is meant to test if a string is split properly across\nanewlineandlong\nword with following text.";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSplitLongLines()
        {
            string html = "<p>If a word with a line feed exists over the line feed boundary then you must respect it.</p>";
            string expected = "If a word with a line feed exists over the line feed boundary then you must\nrespect it.";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRemoveSpacesIfTheyOccurAroundLineFeed()
        {
            string html = "<p>A string of text\nwith \nmultiple\n spaces   \n   that \n \n can be safely removed.</p>";
            string expected = "A string of text\nwith\nmultiple\nspaces\nthat\n\ncan be safely removed.";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRemoveSpacesIfTheyOccurAroundLineFeed2()
        {
            string html = "multiple\n spaces";
            string expected = "multiple\nspaces";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldConsiderNewLineWordIfPreserveNewLines()
        {
            // dotnet seems to treat "\n" as an empty string, no idea why.
            // var allWhitespaceOrEmptyRe = new Regex("^[ \r\t\f]*$", RegexOptions.Multiline);
            // var test = allWhitespaceOrEmptyRe.IsMatch("\n");
            // test.ShouldBeFalse();

            var processor = new WhitespaceProcessor(new HtmlToTextOptions() { preserveNewlines = true });
            processor.testContainsWords("\n").ShouldBeTrue();
            
        }

        [TestMethod]
        public void ShouldProduceEqualResultsRegardlessOfNewlinePositionBetweenBlocks()
        {
            string newlineOutside = "<p>A</p>\n<p>B</p>";
            string newlineInside = "<p>A</p><p>\nB</p>";
            var r1 = defaultConvert.Convert(newlineOutside, new () { preserveNewlines = true });
            var r2 = defaultConvert.Convert(newlineInside, new () { preserveNewlines = true });
            r1.ShouldBe(r2);
        }

        [TestMethod]
        public void ShouldProduceEqualResultsForPreservedNewlinesAndBrTags()
        {
            string nlHtml = "<p>A</p>\n<p>B</p><p>\nC</p>";
            string brHtml = "<p>A</p><br/><p>B</p><p><br/>C</p>";
            var nlResult = defaultConvert.Convert(nlHtml, new () { preserveNewlines = true });
            var brResult = defaultConvert.Convert(brHtml);
            nlResult.ShouldBe(brResult);
        }

        [TestMethod]
        public void ShouldAccountForTrailingLeadingLinebreaksOfAdjacentBlocksEqually()
        {
            // string html = "<p>A</p>\n<div>B</div>\n<div>C</div>\n<p>D</p>";
            // string newlineInside = "A\n\n\nB\n\nC\n\n\nD";
            string html = "<p>A</p>\n<div>B</div>";
            string newlineInside = "A\n\n\nB";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(newlineInside);
        }

        [TestMethod]
        public void ShouldWorkWithMultipleLinebreaksAndInPresenceOfWhitespaces()
        {
            string html = "<p>A</p> \n \n <p>B</p>";
            string newlineInside = "A\n\n\n\nB";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(newlineInside);
        }

        [TestMethod]
        public void ShouldHaveNoSpecialBehaviorInPresenceOfWordsAmongLinebreaks()
        {
            string html = "<p>A</p> \n B \n <p>C</p>";
            string newlineInside = "A\n\n\nB\n\n\nC";
            defaultConvert.Convert(html, new () { preserveNewlines = true }).ShouldBe(newlineInside);
        }
    }
    
    [TestClass]
    public class UnicodeAndHtmlEntitiesTests
    {
        private readonly HtmlToTextConverter defaultConvert = new();

        [TestMethod]
        public void ShouldDecodeUnicodeToEmoji()
        {
            var result = defaultConvert.Convert("&#128514;");
            result.ShouldBe("😂");
        }

        [TestMethod]
        public void ShouldDecodeHtmlEntities()
        {
            var result = defaultConvert.Convert("<span>span</span>, &lt;not a span&gt;");
            result.ShouldBe("span, <not a span>");
        }
    }
    
    
    [TestClass]
    public class LongWordsTests
    {
        private readonly HtmlToTextConverter defaultConvert = new();
    
        [TestMethod]
        public void ShouldSplitLongWordsIfForceWrapOnLimitIsSetExistingLineFeedsConvertedToSpace()
        {
            string html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = "/", forceWrapOnLimit = true } };
            string expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlo\nng word_with_following_text.";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotWrapAStringIfLongWordSplitIsNotSet()
        {
            string html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlongword_with_following_text.</p>";
            string expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlongword_with_following_text.";
            defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotWrapAStringIfWrapCharactersAreSetButNotFoundAndForceWrapOnLimitIsNotSet()
        {
            string html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = "/", forceWrapOnLimit = false } };
            string expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotWrapAStringIfWrapCharactersAreNotSetAndForceWrapOnLimitIsNotSet()
        {
            string html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = null, forceWrapOnLimit = false } };
            string expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapOnTheLastInstanceOfAWrapCharacterBeforeTheWordwrapLimit()
        {
            string html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = "/_", forceWrapOnLimit = false } };
            string expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_\nanewlineandlong word_with_following_text.";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapOnTheLastInstanceOfAWrapCharacterBeforeTheWordwrapLimitContentOfWrapCharactersShouldNotMatter()
        {
            string html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = "/-_", forceWrapOnLimit = false } };
            string expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_\nanewlineandlong word_with_following_text.";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapOnTheLastInstanceOfAWrapCharacterBeforeTheWordwrapLimitOrderOfWrapCharactersShouldNotMatter()
        {
            string html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = "_/", forceWrapOnLimit = false } };
            string expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_\nanewlineandlong word_with_following_text.";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapOnTheLastInstanceOfAWrapCharacterBeforeTheWordwrapLimitShouldPreferenceWrapCharactersInOrder()
        {
            string html = "<p>_This_string_is_meant_to_test_if_a_string_is_split-properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = "-_/", forceWrapOnLimit = false } };
            string expected = "_This_string_is_meant_to_test_if_a_string_is_split-\nproperly_across_anewlineandlong word_with_following_text.";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotWrapAStringThatIsTooShort()
        {
            string html = "<p>https://github.com/werk85/node-html-to-text/blob/master/lib/html-to-text.js</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = "/-", forceWrapOnLimit = false } };
            string expected = "https://github.com/werk85/node-html-to-text/blob/master/lib/html-to-text.js";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapAUrlStringUsingSlash()
        {
            string html = "<p>https://github.com/AndrewFinlay/node-html-to-text/commit/64836a5bd97294a672b24c26cb8a3ccdace41001</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = "/-", forceWrapOnLimit = false } };
            string expected = "https://github.com/AndrewFinlay/node-html-to-text/commit/\n64836a5bd97294a672b24c26cb8a3ccdace41001";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapVeryLongUrlStringsUsingSlash()
        {
            string html = "<p>https://github.com/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/html-to-text.js</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = "/-", forceWrapOnLimit = false } };
            string expected = "https://github.com/werk85/node-html-to-text/blob/master/lib/werk85/\nnode-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/\nwerk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/\nlib/html-to-text.js";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapVeryLongUrlStringsUsingLimit()
        {
            string html = "<p>https://github.com/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/html-to-text.js</p>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = null, forceWrapOnLimit = true } };
            string expected = "https://github.com/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-\ntext/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-t\no-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/html-to-text.js";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHonourPreserveNewlinesAndSplitLongWords()
        {
            string html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { preserveNewlines = true, longWordSplit = new () { wrapCharacters = "/_", forceWrapOnLimit = false } };
            string expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_\nanewlineandlong\nword_with_following_text.";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotPutInExtraLineFeedsIfTheEndOfTheUntouchedLongStringCoincidesWithAPreservedLineFeed()
        {
            string html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            string expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.";
            defaultConvert.Convert(html, new HtmlToTextOptions(){ preserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSplitLongStringsBuriedInLinksAndHideTheHref()
        {
            string html = "<a href=\"http://images.fb.com/2015/12/21/ivete-sangalo-launches-360-music-video-on-facebook/\">http://images.fb.com/2015/12/21/ivete-sangalo-launches-360-music-video-on-facebook/</a>";
            var options = new HtmlToTextOptions
            {
                longWordSplit =
                {
                    wrapCharacters = "/_",
                    forceWrapOnLimit = false
                }
            };
            options.A.options.hideLinkHrefIfSameAsText = true;
        
            string expected = "http://images.fb.com/2015/12/21/\nivete-sangalo-launches-360-music-video-on-facebook/";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSplitLongStringsBuriedInLinksAndShowTheHref()
        {
            string html = "<a href=\"http://images.fb.com/2015/12/21/ivete-sangalo-launches-360-music-video-on-facebook/\">http://images.fb.com/2015/12/21/ivete-sangalo-launches-360-music-video-on-facebook/</a>";
            var options = new HtmlToTextOptions { longWordSplit = new () { wrapCharacters = "/_", forceWrapOnLimit = false } };
            string expected = "http://images.fb.com/2015/12/21/\nivete-sangalo-launches-360-music-video-on-facebook/\n[http://images.fb.com/2015/12/21/\nivete-sangalo-launches-360-music-video-on-facebook/]";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }
    
    }

    [TestClass]
    public class WhitespaceTests
    {
        private readonly HtmlToTextConverter defaultConvert = new();

        [TestMethod]
        public void ShouldNotBeIgnoredInsideAWhitespaceOnlyNode()
        {
            const string html = "foo<span> </span>bar";
            const string expected = "foo bar";
            defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleHtmlCharacterEntitiesForHtmlWhitespaceCharacters()
        {
            const string html = /*html*/ "a<span>&#x0020;</span>b<span>&Tab;</span>c<span>&NewLine;</span>d<span>&#10;</span>e";
            var result = defaultConvert.Convert(html);
            const string expected = "a b c d e";
            result.ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotAddAdditionalWhitespaceAfterSup()
        {
            const string html = "<p>This text contains <sup>superscript</sup> text.</p>";
            var options = new HtmlToTextOptions() { preserveNewlines = true };
            const string expected = "This text contains superscript text.";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleCustomWhitespaceCharacters()
        {
            // No-Break Space - decimal 160, hex \u00a0.
            const string html = /*html*/ "<span>first span\u00a0</span>&nbsp;<span>&#160;last span</span>";
            const string expectedDefault = "first span\u00a0\u00a0\u00a0last span";
            defaultConvert.Convert(html).ShouldBe(expectedDefault);
            var options = new HtmlToTextOptions { whitespaceCharacters = " \t\r\n\f\u200b\u00a0" };
            const string expectedCustom = "first span last span";
            defaultConvert.Convert(html, options).ShouldBe(expectedCustom);
        }

        [TestMethod]
        public void ShouldHandleSpaceAndNewlineCombinationKeepSpaceWhenAndOnlyWhenNeeded()
        {
            const string html = "<span>foo</span> \n<span>bar</span>\n <span>baz</span>";
            const string expectedDefault = "foo bar baz";
            defaultConvert.Convert(html).ShouldBe(expectedDefault);
            const string expectedCustom = "foo\nbar\nbaz";
            var options = new HtmlToTextOptions { preserveNewlines = true };
            defaultConvert.Convert(html, options).ShouldBe(expectedCustom);
        }

        [TestMethod]
        public void ShouldNotHaveExtraSpacesAtTheBeginningForSpaceIndentedHtml()
        {
            const string html = /*html*/ "<html>\n<body>\n    <p>foo</p>\n    <p>bar</p>\n</body>\n</html>";
            const string expected = "foo\n\nbar";
            defaultConvert.Convert(html).ShouldBe(expected);
        }
    }

    [TestClass]
    public class LimitsTests
    {
        private readonly HtmlToTextConverter defaultConvert = new();
    
        [TestMethod]
        public void ShouldRespectDefaultLimitOfMaxInputLength()
        {
            var traceWriter = new StringWriter();
            var traceListener = new TextWriterTraceListener(traceWriter);
            Trace.Listeners.Add(traceListener);
        
            var html = "0123456789".Repeat(2000000);
            var options = new HtmlToTextOptions() { wordwrap = 0 };
            defaultConvert.Convert(html, options).Length.ShouldBe(1 << 24);
        
            traceListener.Flush();
            Trace.Listeners.Remove(traceListener);
            traceWriter.ToString().ShouldBe("Input length 20000000 is above allowed limit of 16777216. Truncating without ellipsis.\n");
        
        }

        [TestMethod]
        public void ShouldRespectCustomMaxInputLength()
        {
            var traceWriter = new StringWriter();
            var traceListener = new TextWriterTraceListener(traceWriter);
            Trace.Listeners.Add(traceListener);

            var html = "0123456789".Repeat(2000000);
            var options = new HtmlToTextOptions { limits = new () { maxInputLength = 42 } };
            defaultConvert.Convert(html, options).Length.ShouldBe(42);
        
            traceListener.Flush();
            Trace.Listeners.Remove(traceListener);
            traceWriter.ToString().ShouldBe("Input length 20000000 is above allowed limit of 42. Truncating without ellipsis.\n");
        }
    }
    

    [TestClass]
    public class HtmlToTextConverterTests
    {
        private readonly HtmlToTextConverter defaultConvert = new();

        [TestMethod]
        public void ShouldHandleLargeNumberOfWbrTagsWithoutStackOverflow()
        {
            string html = "<!DOCTYPE html><html><head></head><body>\n";
            string expected = "";
            for (int i = 0; i < 10000; i++)
            {
                if (i != 0 && i % 80 == 0)
                {
                    expected += "\n";
                }
                expected += "n";
                html += "<wbr>n";
            }
            html += "</body></html>";
            defaultConvert.Convert(html).ShouldBe(expected);
        }

        [Ignore]
        [TestMethod]
        public void ShouldHandleVeryLargeNumberOfWbrTagsWithLimits()
        {
            string html = "<!DOCTYPE html><html><head></head><body>";
            for (int i = 0; i < 70000; i++)
            {
                html += "<wbr>n";
            }
            html += "</body></html>";
            var options = new HtmlToTextOptions
            {
                limits = new()
                {
                    maxChildNodes = 10,
                    ellipsis = "(...)"
                }
            };
            const string expected = "nnnnn(...)";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRespectMaxDepthLimit()
        {
            const string html = @"<!DOCTYPE html><html><head></head><body><span>a<span>b<span>c<span>d</span>e</span>f</span>g<span>h<span>i<span>j</span>k</span>l</span>m</span></body></html>";
            var options = new HtmlToTextOptions
            {
                limits = new()
                {
                    maxDepth = 2,
                    ellipsis = "(...)"
                }
            };
            const string expected = "a(...)g(...)m";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRespectMaxChildNodesLimit()
        {
            const string html = @"<!DOCTYPE html><html><head></head><body><p>a</p><p>b</p><p>c</p><p>d</p><p>e</p><p>f</p><p>g</p><p>h</p><p>i</p><p>j</p></body></html>";
        
            var options = new HtmlToTextOptions();
            options.limits.maxChildNodes = 6;
            options.limits.ellipsis = "(skipped the rest)";
            options.P.options.leadingLineBreaks = 1;
            options.P.options.trailingLineBreaks = 1;
            const string expected = "a\nb\nc\nd\ne\nf\n(skipped the rest)";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotAddEllipsisWhenMaxChildNodesLimitIsExactMatch()
        {
            const string html = @"<!DOCTYPE html><html><head></head><body><p>a</p><p>b</p><p>c</p><p>d</p><p>e</p><p>f</p><p>g</p><p>h</p><p>i</p><p>j</p></body></html>";
            var options = new HtmlToTextOptions();
            options.limits.maxChildNodes = 10;
            options.limits.ellipsis = "can't see me";
            options.P.options.leadingLineBreaks = 1;
            options.P.options.trailingLineBreaks = 1;
        
            const string expected = "a\nb\nc\nd\ne\nf\ng\nh\ni\nj";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldUseDefaultEllipsisValueIfNoneProvided()
        {
            const string html = @"<!DOCTYPE html><html><head></head><body><p>a</p><p>b</p><p>c</p><p>d</p><p>e</p><p>f</p><p>g</p><p>h</p><p>i</p><p>j</p></body></html>";
            var options = new HtmlToTextOptions();
            options.limits.maxChildNodes = 6;
            options.P.options.leadingLineBreaks = 1;
            options.P.options.trailingLineBreaks = 1;
            const string expected = "a\nb\nc\nd\ne\nf\n...";
            defaultConvert.Convert(html, options).ShouldBe(expected);
        }
    }
    
}