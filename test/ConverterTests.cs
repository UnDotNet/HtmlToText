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
        
        private readonly HtmlToTextConverter _defaultConvert = new();

        [TestMethod]
        public void ShouldReturnEmptyInputUnchanged()
        {
            _defaultConvert.Convert("").ShouldBe("");
        }

        [TestMethod]
        public void ShouldReturnEmptyResultIfInputIsNull()
        {
            _defaultConvert.Convert().ShouldBe("");
        }

        [TestMethod]
        public void ShouldReturnPlainTextUnchanged()
        {
            _defaultConvert.Convert("Hello world!").ShouldBe("Hello world!");
        }
    }

    [TestClass]
    public class SkippedHtmlContent
    {
        private readonly HtmlToTextConverter _defaultConvert = new();
        
        [TestMethod]
        public void ShouldIgnoreHtmlComments()
        {
            var html = @"<!--[^-]*-->
                        <!-- <h1>Hello World</h1> -->
                        text";
            _defaultConvert.Convert(html).ShouldBe("text");
        }

        [TestMethod]
        public void ShouldIgnoreScripts()
        {
            var html = @"<script src=""javascript.js""></script>
                        <script>
                          console.log(""Hello World!"");
                        </script>
                        <script id=""data"" type=""application/json"">{'userId':1234,'userName':'John Doe','memberSince':'2000-01-01T00:00:00.000Z'}</script>
                        text";
            _defaultConvert.Convert(html).ShouldBe("text");
        }

        [TestMethod]
        public void ShouldIgnoreStyles()
        {
            var html = @"<link href=""main.css"" rel=""stylesheet"">
                        <style type=""text/css"" media=""all and (max-width: 500px)"">
                          p { color: #26b72b; }
                        </style>
                        text";
            _defaultConvert.Convert(html).ShouldBe("text");
        }

        [TestMethod]
        public void ShouldNotBreakAfterSpecialTagFollowedByEntity()
        {
            var html = @"<style>a{}</style>&apos;<br/><span>text</span>";
            _defaultConvert.Convert(html).ShouldBe("'\ntext");
        }
    }

    [TestClass]
    public class WordWrapOption
    {
        private readonly HtmlToTextConverter _defaultConvert = new();

        [TestMethod]
        public void ShouldWordWrapAt80CharactersByDefault()
        {
            var longStr = "111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888 999999999";
            _defaultConvert.Convert(longStr).ShouldBe("111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888\n999999999");
        }

        [TestMethod]
        public void ShouldWordWrapAtGivenNumberOfCharacters()
        {
            var longStr = "111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888 999999999";
            _defaultConvert.Convert(longStr, new HtmlToTextOptions { Wordwrap = 20 }).ShouldBe("111111111 222222222\n333333333 444444444\n555555555 666666666\n777777777 888888888\n999999999");
            _defaultConvert.Convert(longStr, new HtmlToTextOptions { Wordwrap = 50 }).ShouldBe("111111111 222222222 333333333 444444444 555555555\n666666666 777777777 888888888 999999999");
            _defaultConvert.Convert(longStr, new HtmlToTextOptions { Wordwrap = 70 }).ShouldBe("111111111 222222222 333333333 444444444 555555555 666666666 777777777\n888888888 999999999");
        }

        [TestMethod]
        public void ShouldNotWordWrapWhenGivenNull()
        {
            var longStr = "111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888 999999999";
            _defaultConvert.Convert(longStr, new HtmlToTextOptions { Wordwrap = null }).ShouldBe(longStr);
        }

        [TestMethod]
        public void ShouldNotWordWrapWhenGivenFalse()
        {
            var longStr = "111111111 222222222 333333333 444444444 555555555 666666666 777777777 888888888 999999999";
            _defaultConvert.Convert(longStr, new HtmlToTextOptions { Wordwrap = 0 }).ShouldBe(longStr);
        }

        [TestMethod]
        public void ShouldNotExceedTheLineWidthWhenProcessingEmbeddedFormatTags()
        {
            var html = "<p><strong>This text isn't counted</strong> when calculating where to break a string for 80 character line lengths.</p>";
            var expected = "This text isn't counted when calculating where to break a string for 80\ncharacter line lengths.";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWorkWithALongStringContainingLineFeeds()
        {
            var html = "<p>If a word with a line feed exists over the line feed boundary then\nyou\nmust\nrespect it.</p>";
            var expected = "If a word with a line feed exists over the line feed boundary then you must\nrespect it.";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotWronglyTruncateLinesWhenProcessingEmbeddedFormatTags()
        {
            var html = "<p><strong>This text isn't counted</strong> when calculating where to break a string for 80 character line lengths.  However it can affect where the next line breaks and this could lead to having an early line break</p>";
            var expected = "This text isn't counted when calculating where to break a string for 80\ncharacter line lengths. However it can affect where the next line breaks and\nthis could lead to having an early line break";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotExceedTheLineWidthWhenProcessingAnchorTags()
        {
            var html = "<p>We appreciate your business. And we hope you'll check out our <a href=\"https://example.com/\">new products</a>!</p>";
            var expected = "We appreciate your business. And we hope you'll check out our new products\n[https://example.com/]!";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHonourLineFeedsFromALongWordAcrossTheWrapWhereTheLineFeedIsBeforeTheWrap()
        {
            var html = "<p>This string is meant to test if a string is split properly across a\nnewlineandlongword with following text.</p>";
            var expected = "This string is meant to test if a string is split properly across a\nnewlineandlongword with following text.";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRemoveLineFeedsFromALongWordAcrossTheWrapWhereTheLineFeedIsAfterTheWrap()
        {
            var html = "<p>This string is meant to test if a string is split properly across anewlineandlong\nword with following text.</p>";
            var expected = "This string is meant to test if a string is split properly across\nanewlineandlong word with following text.";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }
    }

    [TestClass]
    public class PreserveNewlinesTests
    {
        private readonly HtmlToTextConverter _defaultConvert = new();
        
        [TestMethod]
        public void ShouldNotPreserveNewlinesByDefault()
        {
            var html = "<p\n>One\nTwo\nThree</p>";
            var expected = "One Two Three";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldPreserveNewlinesWhenProvidedWithTruthyValue()
        {
            var html = "<p\n>One\nTwo\nThree</p>";
            var expected = "One\nTwo\nThree";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldPreserveLineFeedsInALongWrappingStringContainingLineFeeds()
        {
            var html = "<p>If a word with a line feed exists over the line feed boundary then\nyou\nmust\nrespect it.</p>";
            var expected = "If a word with a line feed exists over the line feed boundary then\nyou\nmust\nrespect it.";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldPreserveLineFeedsInALongStringContainingLineFeedsAcrossTheWrap()
        {
            var html = "<p>If a word with a line feed exists over the line feed boundary then\nyou must respect it.</p>";
            var expected = "If a word with a line feed exists over the line feed boundary then\nyou must respect it.";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldPreserveLineFeedsInALongStringContainingLineFeedsAcrossTheWrapWithALineFeedBefore80Chars()
        {
            var html = "<p>This string is meant to test if a string is split properly across a\nnewlineandlongword with following text.</p>";
            var expected = "This string is meant to test if a string is split properly across a\nnewlineandlongword with following text.";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldPreserveLineFeedsInALongStringContainingLineFeedsAcrossTheWrapWithALineFeedAfter80Chars()
        {
            var html = "<p>This string is meant to test if a string is split properly across anewlineandlong\nword with following text.</p>";
            var expected = "This string is meant to test if a string is split properly across\nanewlineandlong\nword with following text.";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSplitLongLines()
        {
            var html = "<p>If a word with a line feed exists over the line feed boundary then you must respect it.</p>";
            var expected = "If a word with a line feed exists over the line feed boundary then you must\nrespect it.";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRemoveSpacesIfTheyOccurAroundLineFeed()
        {
            var html = "<p>A string of text\nwith \nmultiple\n spaces   \n   that \n \n can be safely removed.</p>";
            var expected = "A string of text\nwith\nmultiple\nspaces\nthat\n\ncan be safely removed.";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRemoveSpacesIfTheyOccurAroundLineFeed2()
        {
            var html = "multiple\n spaces";
            var expected = "multiple\nspaces";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldConsiderNewLineWordIfPreserveNewLines()
        {
            // dotnet seems to treat "\n" as an empty string, no idea why.
            // var allWhitespaceOrEmptyRe = new Regex("^[ \r\t\f]*$", RegexOptions.Multiline);
            // var test = allWhitespaceOrEmptyRe.IsMatch("\n");
            // test.ShouldBeFalse();

            var processor = new WhitespaceProcessor(new HtmlToTextOptions() { PreserveNewlines = true });
            processor.TestContainsWords("\n").ShouldBeTrue();
            
        }

        [TestMethod]
        public void ShouldProduceEqualResultsRegardlessOfNewlinePositionBetweenBlocks()
        {
            var newlineOutside = "<p>A</p>\n<p>B</p>";
            var newlineInside = "<p>A</p><p>\nB</p>";
            var r1 = _defaultConvert.Convert(newlineOutside, new () { PreserveNewlines = true });
            var r2 = _defaultConvert.Convert(newlineInside, new () { PreserveNewlines = true });
            r1.ShouldBe(r2);
        }

        [TestMethod]
        public void ShouldProduceEqualResultsForPreservedNewlinesAndBrTags()
        {
            var nlHtml = "<p>A</p>\n<p>B</p><p>\nC</p>";
            var brHtml = "<p>A</p><br/><p>B</p><p><br/>C</p>";
            var nlResult = _defaultConvert.Convert(nlHtml, new () { PreserveNewlines = true });
            var brResult = _defaultConvert.Convert(brHtml);
            nlResult.ShouldBe(brResult);
        }

        [TestMethod]
        public void ShouldAccountForTrailingLeadingLinebreaksOfAdjacentBlocksEqually()
        {
            // string html = "<p>A</p>\n<div>B</div>\n<div>C</div>\n<p>D</p>";
            // string newlineInside = "A\n\n\nB\n\nC\n\n\nD";
            var html = "<p>A</p>\n<div>B</div>";
            var newlineInside = "A\n\n\nB";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(newlineInside);
        }

        [TestMethod]
        public void ShouldWorkWithMultipleLinebreaksAndInPresenceOfWhitespaces()
        {
            var html = "<p>A</p> \n \n <p>B</p>";
            var newlineInside = "A\n\n\n\nB";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(newlineInside);
        }

        [TestMethod]
        public void ShouldHaveNoSpecialBehaviorInPresenceOfWordsAmongLinebreaks()
        {
            var html = "<p>A</p> \n B \n <p>C</p>";
            var newlineInside = "A\n\n\nB\n\n\nC";
            _defaultConvert.Convert(html, new () { PreserveNewlines = true }).ShouldBe(newlineInside);
        }
    }
    
    [TestClass]
    public class UnicodeAndHtmlEntitiesTests
    {
        private readonly HtmlToTextConverter _defaultConvert = new();

        [TestMethod]
        public void ShouldDecodeUnicodeToEmoji()
        {
            var result = _defaultConvert.Convert("&#128514;");
            result.ShouldBe("😂");
        }

        [TestMethod]
        public void ShouldDecodeHtmlEntities()
        {
            var result = _defaultConvert.Convert("<span>span</span>, &lt;not a span&gt;");
            result.ShouldBe("span, <not a span>");
        }
    }
    
    
    [TestClass]
    public class LongWordsTests
    {
        private readonly HtmlToTextConverter _defaultConvert = new();
    
        [TestMethod]
        public void ShouldSplitLongWordsIfForceWrapOnLimitIsSetExistingLineFeedsConvertedToSpace()
        {
            var html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = "/", ForceWrapOnLimit = true } };
            var expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlo\nng word_with_following_text.";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotWrapAStringIfLongWordSplitIsNotSet()
        {
            var html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlongword_with_following_text.</p>";
            var expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlongword_with_following_text.";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotWrapAStringIfWrapCharactersAreSetButNotFoundAndForceWrapOnLimitIsNotSet()
        {
            var html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = "/", ForceWrapOnLimit = false } };
            var expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotWrapAStringIfWrapCharactersAreNotSetAndForceWrapOnLimitIsNotSet()
        {
            var html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = null, ForceWrapOnLimit = false } };
            var expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapOnTheLastInstanceOfAWrapCharacterBeforeTheWordwrapLimit()
        {
            var html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = "/_", ForceWrapOnLimit = false } };
            var expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_\nanewlineandlong word_with_following_text.";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapOnTheLastInstanceOfAWrapCharacterBeforeTheWordwrapLimitContentOfWrapCharactersShouldNotMatter()
        {
            var html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = "/-_", ForceWrapOnLimit = false } };
            var expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_\nanewlineandlong word_with_following_text.";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapOnTheLastInstanceOfAWrapCharacterBeforeTheWordwrapLimitOrderOfWrapCharactersShouldNotMatter()
        {
            var html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = "_/", ForceWrapOnLimit = false } };
            var expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_\nanewlineandlong word_with_following_text.";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapOnTheLastInstanceOfAWrapCharacterBeforeTheWordwrapLimitShouldPreferenceWrapCharactersInOrder()
        {
            var html = "<p>_This_string_is_meant_to_test_if_a_string_is_split-properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = "-_/", ForceWrapOnLimit = false } };
            var expected = "_This_string_is_meant_to_test_if_a_string_is_split-\nproperly_across_anewlineandlong word_with_following_text.";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotWrapAStringThatIsTooShort()
        {
            var html = "<p>https://github.com/werk85/node-html-to-text/blob/master/lib/html-to-text.js</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = "/-", ForceWrapOnLimit = false } };
            var expected = "https://github.com/werk85/node-html-to-text/blob/master/lib/html-to-text.js";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapAUrlStringUsingSlash()
        {
            var html = "<p>https://github.com/AndrewFinlay/node-html-to-text/commit/64836a5bd97294a672b24c26cb8a3ccdace41001</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = "/-", ForceWrapOnLimit = false } };
            var expected = "https://github.com/AndrewFinlay/node-html-to-text/commit/\n64836a5bd97294a672b24c26cb8a3ccdace41001";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapVeryLongUrlStringsUsingSlash()
        {
            var html = "<p>https://github.com/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/html-to-text.js</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = "/-", ForceWrapOnLimit = false } };
            var expected = "https://github.com/werk85/node-html-to-text/blob/master/lib/werk85/\nnode-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/\nwerk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/\nlib/html-to-text.js";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapVeryLongUrlStringsUsingLimit()
        {
            var html = "<p>https://github.com/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/html-to-text.js</p>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = null, ForceWrapOnLimit = true } };
            var expected = "https://github.com/werk85/node-html-to-text/blob/master/lib/werk85/node-html-to-\ntext/blob/master/lib/werk85/node-html-to-text/blob/master/lib/werk85/node-html-t\no-text/blob/master/lib/werk85/node-html-to-text/blob/master/lib/html-to-text.js";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHonourPreserveNewlinesAndSplitLongWords()
        {
            var html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var options = new HtmlToTextOptions { PreserveNewlines = true, LongWordSplit = new () { WrapCharacters = "/_", ForceWrapOnLimit = false } };
            var expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_\nanewlineandlong\nword_with_following_text.";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotPutInExtraLineFeedsIfTheEndOfTheUntouchedLongStringCoincidesWithAPreservedLineFeed()
        {
            var html = "<p>_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.</p>";
            var expected = "_This_string_is_meant_to_test_if_a_string_is_split_properly_across_anewlineandlong\nword_with_following_text.";
            _defaultConvert.Convert(html, new HtmlToTextOptions(){ PreserveNewlines = true }).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSplitLongStringsBuriedInLinksAndHideTheHref()
        {
            var html = "<a href=\"https://images.fb.com/2015/12/21/ivete-sangalo-launches-360-music-video-on-facebook/\">https://images.fb.com/2015/12/21/ivete-sangalo-launches-360-music-video-on-facebook/</a>";
            var options = new HtmlToTextOptions
            {
                LongWordSplit =
                {
                    WrapCharacters = "/_",
                    ForceWrapOnLimit = false
                }
            };
            options.A.Options.HideLinkHrefIfSameAsText = true;
        
            var expected = "https://images.fb.com/2015/12/21/\nivete-sangalo-launches-360-music-video-on-facebook/";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSplitLongStringsBuriedInLinksAndShowTheHref()
        {
            var html = "<a href=\"https://images.fb.com/2015/12/21/ivete-sangalo-launches-360-music-video-on-facebook/\">https://images.fb.com/2015/12/21/ivete-sangalo-launches-360-music-video-on-facebook/</a>";
            var options = new HtmlToTextOptions { LongWordSplit = new () { WrapCharacters = "/_", ForceWrapOnLimit = false } };
            var expected = "https://images.fb.com/2015/12/21/\nivete-sangalo-launches-360-music-video-on-facebook/\n[https://images.fb.com/2015/12/21/\nivete-sangalo-launches-360-music-video-on-facebook/]";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }
    
    }

    [TestClass]
    public class WhitespaceTests
    {
        private readonly HtmlToTextConverter _defaultConvert = new();

        [TestMethod]
        public void ShouldNotBeIgnoredInsideAWhitespaceOnlyNode()
        {
            var html = "foo<span> </span>bar";
            var expected = "foo bar";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleHtmlCharacterEntitiesForHtmlWhitespaceCharacters()
        {
            var html = /*html*/ "a<span>&#x0020;</span>b<span>&Tab;</span>c<span>&NewLine;</span>d<span>&#10;</span>e";
            var result = _defaultConvert.Convert(html);
            var expected = "a b c d e";
            result.ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotAddAdditionalWhitespaceAfterSup()
        {
            var html = "<p>This text contains <sup>superscript</sup> text.</p>";
            var options = new HtmlToTextOptions() { PreserveNewlines = true };
            var expected = "This text contains superscript text.";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleCustomWhitespaceCharacters()
        {
            // No-Break Space - decimal 160, hex \u00a0.
            var html = /*html*/ "<span>first span\u00a0</span>&nbsp;<span>&#160;last span</span>";
            var expectedDefault = "first span\u00a0\u00a0\u00a0last span";
            _defaultConvert.Convert(html).ShouldBe(expectedDefault);
            var options = new HtmlToTextOptions { WhitespaceCharacters = " \t\r\n\f\u200b\u00a0" };
            var expectedCustom = "first span last span";
            _defaultConvert.Convert(html, options).ShouldBe(expectedCustom);
        }

        [TestMethod]
        public void ShouldHandleSpaceAndNewlineCombinationKeepSpaceWhenAndOnlyWhenNeeded()
        {
            var html = "<span>foo</span> \n<span>bar</span>\n <span>baz</span>";
            var expectedDefault = "foo bar baz";
            _defaultConvert.Convert(html).ShouldBe(expectedDefault);
            var expectedCustom = "foo\nbar\nbaz";
            var options = new HtmlToTextOptions { PreserveNewlines = true };
            _defaultConvert.Convert(html, options).ShouldBe(expectedCustom);
        }

        [TestMethod]
        public void ShouldNotHaveExtraSpacesAtTheBeginningForSpaceIndentedHtml()
        {
            var html = /*html*/ "<html>\n<body>\n    <p>foo</p>\n    <p>bar</p>\n</body>\n</html>";
            var expected = "foo\n\nbar";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }
    }

    [TestClass]
    public class LimitsTests
    {
        private readonly HtmlToTextConverter _defaultConvert = new();
    
        [TestMethod]
        public void ShouldRespectDefaultLimitOfMaxInputLength()
        {
            var traceWriter = new StringWriter();
            var traceListener = new TextWriterTraceListener(traceWriter);
            Trace.Listeners.Add(traceListener);
        
            var html = "0123456789".Repeat(2000000);
            var options = new HtmlToTextOptions() { Wordwrap = 0 };
            _defaultConvert.Convert(html, options).Length.ShouldBe(1 << 24);
        
            traceListener.Flush();
            Trace.Listeners.Remove(traceListener);
            traceWriter.ToString().Trim().ShouldBe("Input length 20000000 is above allowed limit of 16777216. Truncating without ellipsis.");
        
        }

        [TestMethod]
        public void ShouldRespectCustomMaxInputLength()
        {
            var traceWriter = new StringWriter();
            var traceListener = new TextWriterTraceListener(traceWriter);
            Trace.Listeners.Add(traceListener);

            var html = "0123456789".Repeat(2000000);
            var options = new HtmlToTextOptions { Limits = new () { MaxInputLength = 42 } };
            _defaultConvert.Convert(html, options).Length.ShouldBe(42);
        
            traceListener.Flush();
            Trace.Listeners.Remove(traceListener);
            traceWriter.ToString().Trim().ShouldBe("Input length 20000000 is above allowed limit of 42. Truncating without ellipsis.");
        }
    }
    

    [TestClass]
    public class HtmlToTextConverterTests
    {
        private readonly HtmlToTextConverter _defaultConvert = new();

        [TestMethod]
        public void ShouldHandleLargeNumberOfWbrTagsWithoutStackOverflow()
        {
            var html = "<!DOCTYPE html><html><head></head><body>\n";
            var expected = "";
            for (var i = 0; i < 10000; i++)
            {
                if (i != 0 && i % 80 == 0)
                {
                    expected += "\n";
                }
                expected += "n";
                html += "<wbr>n";
            }
            html += "</body></html>";
            _defaultConvert.Convert(html).ShouldBe(expected);
        }

        [Ignore]
        [TestMethod]
        public void ShouldHandleVeryLargeNumberOfWbrTagsWithLimits()
        {
            var html = "<!DOCTYPE html><html><head></head><body>";
            for (var i = 0; i < 70000; i++)
            {
                html += "<wbr>n";
            }
            html += "</body></html>";
            var options = new HtmlToTextOptions
            {
                Limits = new()
                {
                    MaxChildNodes = 10,
                    Ellipsis = "(...)"
                }
            };
            var expected = "nnnnn(...)";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRespectMaxDepthLimit()
        {
            var html = @"<!DOCTYPE html><html><head></head><body><span>a<span>b<span>c<span>d</span>e</span>f</span>g<span>h<span>i<span>j</span>k</span>l</span>m</span></body></html>";
            var options = new HtmlToTextOptions
            {
                Limits = new()
                {
                    MaxDepth = 2,
                    Ellipsis = "(...)"
                }
            };
            var expected = "a(...)g(...)m";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRespectMaxChildNodesLimit()
        {
            var html = @"<!DOCTYPE html><html><head></head><body><p>a</p><p>b</p><p>c</p><p>d</p><p>e</p><p>f</p><p>g</p><p>h</p><p>i</p><p>j</p></body></html>";
        
            var options = new HtmlToTextOptions();
            options.Limits.MaxChildNodes = 6;
            options.Limits.Ellipsis = "(skipped the rest)";
            options.P.Options.LeadingLineBreaks = 1;
            options.P.Options.TrailingLineBreaks = 1;
            var expected = "a\nb\nc\nd\ne\nf\n(skipped the rest)";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotAddEllipsisWhenMaxChildNodesLimitIsExactMatch()
        {
            var html = @"<!DOCTYPE html><html><head></head><body><p>a</p><p>b</p><p>c</p><p>d</p><p>e</p><p>f</p><p>g</p><p>h</p><p>i</p><p>j</p></body></html>";
            var options = new HtmlToTextOptions();
            options.Limits.MaxChildNodes = 10;
            options.Limits.Ellipsis = "can't see me";
            options.P.Options.LeadingLineBreaks = 1;
            options.P.Options.TrailingLineBreaks = 1;
        
            var expected = "a\nb\nc\nd\ne\nf\ng\nh\ni\nj";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldUseDefaultEllipsisValueIfNoneProvided()
        {
            var html = @"<!DOCTYPE html><html><head></head><body><p>a</p><p>b</p><p>c</p><p>d</p><p>e</p><p>f</p><p>g</p><p>h</p><p>i</p><p>j</p></body></html>";
            var options = new HtmlToTextOptions();
            options.Limits.MaxChildNodes = 6;
            options.P.Options.LeadingLineBreaks = 1;
            options.P.Options.TrailingLineBreaks = 1;
            var expected = "a\nb\nc\nd\ne\nf\n...";
            _defaultConvert.Convert(html, options).ShouldBe(expected);
        }
    }
    
}