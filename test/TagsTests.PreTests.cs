
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    
    [TestClass]
    public class PreTests
    {
        private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);
    
        [TestMethod]
        public void ShouldSupportSimplePreformattedText()
        {
            var html = "<P>Code fragment:</P><PRE>  body {\n    color: red;\n  }</PRE>";
            var expected = "Code fragment:\n\n  body {\n    color: red;\n  }";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportPreformattedTextWithInnerTags()
        {
            var html = /*html*/ """
                                <p>Code fragment:</p>
                                <pre><code>  var total = 0;
                                
                                  <em style="color: green;">// Add 1 to total and display in a paragraph</em>
                                  <strong style="color: blue;">document.write('&lt;p&gt;Sum: ' + (total + 1) + '&lt;/p&gt;');</strong></code></pre>
                                """;
            // string html = /*html*/"<p>Code fragment:</p>\n<pre><code>  var total = 0;\n  <em style=\"color: green;\">// Add 1 to total and display in a paragraph</em>\n  <strong style=\"color: blue;\">document.write('&lt;p&gt;Sum: ' + (total + 1) + '&lt;/p&gt;');</strong></code></pre>";
            var expected = "Code fragment:\n\n  var total = 0;\n\n  // Add 1 to total and display in a paragraph\n  document.write('<p>Sum: ' + (total + 1) + '</p>');";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportPreformattedTextWithLineBreakTags()
        {
            var html = "<pre> line 1 <br/> line 2 </pre>";
            var expected = " line 1 \n line 2 ";
            HtmlToText(html).ShouldBe(expected);
        }

    
        [TestMethod]
        public void ShouldSupportPreformattedTextWithATable()
        {
            var html = @"<pre><table>
    <tr>
        <td>[a&#32;&#32;&#32;
     </td>
        <td>  b&#32;&#32;
     </td>
        <td>   c]
     </td>
    </tr>
    <tr>
        <td>&#32;&#32;&#32;&#32;&#32;
   d]</td>
        <td>&#32;&#32;&#32;&#32;&#32;
  e  </td>
        <td>&#32;&#32;&#32;&#32;&#32;
[f   </td>
    </tr>
</table></pre>";
            var expected =
                "[a        b        c]\n" +
                "                     \n" +
                "                     \n" +
                "   d]     e     [f   ";
            var options = new HtmlToTextOptions
            {
                Table =
                {
                    Format = "dataTable"
                }
            };
            HtmlToText(html, options).ShouldBe(expected);
        }

    }
}