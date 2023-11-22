
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    
    [TestClass]
    public class PreTests
    {
        private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);
    
        [TestMethod]
        public void ShouldSupportSimplePreformattedText()
        {
            string html = "<P>Code fragment:</P><PRE>  body {\n    color: red;\n  }</PRE>";
            string expected = "Code fragment:\n\n  body {\n    color: red;\n  }";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportPreformattedTextWithInnerTags()
        {
            string html = /*html*/ """
                                   <p>Code fragment:</p>
                                   <pre><code>  var total = 0;
                                   
                                     <em style="color: green;">// Add 1 to total and display in a paragraph</em>
                                     <strong style="color: blue;">document.write('&lt;p&gt;Sum: ' + (total + 1) + '&lt;/p&gt;');</strong></code></pre>
                                   """;
            // string html = /*html*/"<p>Code fragment:</p>\n<pre><code>  var total = 0;\n  <em style=\"color: green;\">// Add 1 to total and display in a paragraph</em>\n  <strong style=\"color: blue;\">document.write('&lt;p&gt;Sum: ' + (total + 1) + '&lt;/p&gt;');</strong></code></pre>";
            string expected = "Code fragment:\n\n  var total = 0;\n\n  // Add 1 to total and display in a paragraph\n  document.write('<p>Sum: ' + (total + 1) + '</p>');";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportPreformattedTextWithLineBreakTags()
        {
            string html = "<pre> line 1 <br/> line 2 </pre>";
            string expected = " line 1 \n line 2 ";
            htmlToText(html).ShouldBe(expected);
        }

    
        [TestMethod]
        public void ShouldSupportPreformattedTextWithATable()
        {
            string html = @"<pre><table>
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
            string expected =
                "[a        b        c]\n" +
                "                     \n" +
                "                     \n" +
                "   d]     e     [f   ";
            var options = new HtmlToTextOptions();
            options.Table.format = "dataTable";
            htmlToText(html, options).ShouldBe(expected);
        }

    }
}