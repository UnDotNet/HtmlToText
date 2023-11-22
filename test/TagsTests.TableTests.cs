// ReSharper disable StringLiteralTypo

// ReSharper disable UseObjectOrCollectionInitializer
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{

    [TestClass]
    public class TablesTests
    {
        private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldHandleBasicTable()
        {
            var html = "Good morning Jacob, " +
                       "<TABLE>" +
                       "<TBODY>" +
                       "<TR>" +
                       "<TD>Lorem ipsum dolor sit amet.</TD>" +
                       "</TR>" +
                       "</TBODY>" +
                       "</TABLE>";

            var expected = "Good morning Jacob,\n\nLorem ipsum dolor sit amet.";

            var options = new HtmlToTextOptions();
            //options.Table.format = "dataTable";
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleBasicDataTable()
        {
            var html = "Good morning Jacob, " +
                       "<TABLE>" +
                       "<TBODY>" +
                       "<TR>" +
                       "<TD>Lorem ipsum dolor sit amet.</TD>" +
                       "</TR>" +
                       "</TBODY>" +
                       "</TABLE>";

            var expected = "Good morning Jacob,\n\nLorem ipsum dolor sit amet.";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleBasicDataTableWithTwoRows()
        {
            var html = "Good morning Jacob, " +
                       "<TABLE>" +
                       "<TBODY>" +
                       "<TR>" +
                       "<TD>Lorem ipsum dolor sit amet.</TD>" +
                       "</TR>" +
                       "<TR>" +
                       "<TD>Row two.</TD>" +
                       "</TR>" +
                       "</TBODY>" +
                       "</TABLE>";

            var expected = "Good morning Jacob,\n\nLorem ipsum dolor sit amet.\nRow two.";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";
            HtmlToText(html, options).ShouldBe(expected);
        }
    
    
        [TestMethod]
        public void ShouldHandleTwoColumnTable()
        {
            var html = @"
<table>
    <tr>
        <td>a</td><td>a</td>
    </tr>
    <tr>
        <td>b</td><td>b</td>
    </tr>
    <tr>
        <td>c</td><td>c</td>
    </tr>
</table>";

            var expected =
                "a   a\n" +
                "b   b\n" +
                "c   c";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";
        
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleThreeColumnTable()
        {
            var html = @"
<table>
    <tr>
        <td>a</td><td>a</td><td>a</td>
    </tr>
    <tr>
        <td>b</td><td>b</td><td>b</td>
    </tr>
    <tr>
        <td>c</td><td>c</td><td>c</td>
    </tr>
</table>";

            var expected =
                "a   a   a\n" +
                "b   b   b\n" +
                "c   c   c";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";
        
            HtmlToText(html, options).ShouldBe(expected);
        }

    
        [TestMethod]
        public void ShouldHandleCenterTagInTables()
        {
            var html = "Good morning Jacob, " +
                       "<TABLE>" +
                       "<CENTER>" +
                       "<TBODY>" +
                       "<TR>" +
                       "<TD>Lorem ipsum dolor sit amet.</TD>" +
                       "</TR>" +
                       "</CENTER>" +
                       "</TBODY>" +
                       "</TABLE>";

            var expected = "Good morning Jacob,\n\nLorem ipsum dolor sit amet.";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";
            HtmlToText(html, options).ShouldBe(expected);
        }
    
        [TestMethod]
        public void ShouldHandleTwoColumnWithColspanTable()
        {
            var html = @"
<table>
    <tr>
        <td colspan=""2"">a</td>
    </tr>
    <tr>
        <td>b</td><td>b</td>
    </tr>
    <tr>
        <td>c</td><td>c</td>
    </tr>
</table>";

            var expected =
                "a\n" +
                "b   b\n" +
                "c   c";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";
        
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleThreeColumnTableWithRowSpan()
        {
            var html = @"
<table>
    <tr>
        <td>b</td><td rowspan=""2"">b</td><td>b</td>
    </tr>
    <tr>
        <td>c</td><td>c</td>
    </tr>
</table>";

            var expected =
                "b   b   b\n" +
                "c       c";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";
        
            HtmlToText(html, options).ShouldBe(expected);
        }


        [TestMethod]
        public void ShouldHandleNonIntegerColspanOnTdElementGracefully()
        {
            var html = "Good morning Jacob," +
                       "<table>" +
                       "<tbody>" +
                       "<tr>" +
                       "<td colspan=\"abc\">Lorem ipsum dolor sit amet.</td>" +
                       "</tr>" +
                       "</tbody>" +
                       "</table>";

            var expected = "Good morning Jacob,\n\nLorem ipsum dolor sit amet.";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";

            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldHandleTablesWithColSpansAndRowSpans()
        {
            var html = @"
<table>
    <tr>
        <td colspan=""2"" rowspan=""3"">aa<br/>aa<br/>aa</td>
        <td colspan=""1"" rowspan=""3"">b<br/>b<br/>b</td>
        <td colspan=""4"" rowspan=""2"">cccc<br/>cccc</td>
        <td colspan=""1"" rowspan=""4"">d<br/>d<br/>d<br/>d</td>
    </tr>
    <tr></tr>
    <tr>
        <td colspan=""2"" rowspan=""3"">ee<br/>ee<br/>ee</td>
        <td colspan=""2"" rowspan=""2"">ff<br/>ff</td>
    </tr>
    <tr>
        <td colspan=""3"" rowspan=""1"">ggg</td>
    </tr>
    <tr>
        <td colspan=""1"" rowspan=""2"">h<br/>h</td>
        <td colspan=""2"" rowspan=""2"">ii<br/>ii</td>
        <td colspan=""3"" rowspan=""1"">jjj</td>
    </tr>
    <tr>
        <td colspan=""1"" rowspan=""2"">k<br/>k</td>
        <td colspan=""2"" rowspan=""2"">ll<br/>ll</td>
        <td colspan=""2"" rowspan=""1"">mm</td>
    </tr>
    <tr>
        <td colspan=""2"" rowspan=""2"">nn<br/>nn</td>
        <td colspan=""1"" rowspan=""1"">o</td>
        <td colspan=""2"" rowspan=""2"">pp<br/>pp</td>
    </tr>
    <tr>
        <td colspan=""4"" rowspan=""1"">qqqq</td>
    </tr>
</table>";

            var expected =
                "aa   b   cccc      d\n" +
                "aa   b   cccc      d\n" +
                "aa   b   ee   ff   d\n" +
                "ggg      ee   ff   d\n" +
                "h   ii   ee   jjj\n" +
                "h   ii   k   ll   mm\n" +
                "nn   o   k   ll   pp\n" +
                "nn   qqqq         pp";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";

            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportCustomSpacingForTables()
        {
            var html = @"
<table>
    <tr>
        <td colspan=""2"" rowspan=""2"">aa<br/>aa</td>
        <td>b</td>
    </tr>
    <tr>
        <td>c</td>
    </tr>
    <tr>
        <td>d</td>
        <td>e</td>
        <td>f</td>
    </tr>
</table>";

            var expected =
                "aa  b\n" +
                "aa\n" +
                "\n" +
                "    c\n" +
                "\n" +
                "\n" +
                "d e f";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";
            options.Table.Options.ColSpacing = 1;
            options.Table.Options.RowSpacing = 2;
        
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldSupportZeroColumnSpacing()
        {
            var html = @"
<table>
    <tr>
        <td colspan=""2"" rowspan=""2"">aa<br/>aa</td>
        <td>b</td>
    </tr>
    <tr>
        <td>c</td>
    </tr>
    <tr>
        <td>d</td>
        <td>e</td>
        <td>f</td>
    </tr>
</table>";

            var expected =
                "aab\n" +
                "aac\n" +
                "def";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";
            options.Table.Options.ColSpacing = 0;
        
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldProperlyAlignColumnsInTablesWithTheadTfoot()
        {
            var html = @"
<table>
    <thead>
        <tr>
            <td>aaaaaaaaa</td>
            <td colspan=""2"">b</td>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>ccc</td>
            <td>ddd</td>
            <td>eee</td>
        </tr>
    </tbody>
    <tfoot>
        <tr>
            <td colspan=""2"">f</td>
            <td>ggggggggg</td>
        </tr>
    </tfoot>
</table>";

            var expected =
                "aaaaaaaaa   b\n" +
                "ccc         ddd   eee\n" +
                "f                 ggggggggg";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";

            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldRenderBlockLevelElementsInsideTableCellsProperly()
        {
            var html = @"
<table>
    <tr>
        <td><h1>hEaDeR</h1></td>
        <td><blockquote>A quote<br/>from somewhere.</blockquote></td>
    </tr>
    <tr>
    <td>
        <pre>   preformatted...        ...text   </pre>
    </td>
    <td>
        <ol>
            <li>list item one</li>
            <li>list item two</li>
        </ol>
    </td>
    </tr>
</table>";

            var expected =
                "HEADER                                 > A quote\n" +
                "                                       > from somewhere.\n" +
                "   preformatted...        ...text       1. list item one\n" +
                "                                        2. list item two";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";

            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldWrapTableContentsToCustomMaxColumnWidth()
        {
            var html = @"
<table>
    <tr>
        <td>short</td>
        <td>short</td>
        <td>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.</td>
    </tr>
    <tr>
        <td>short</td>
        <td>Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.</td>
        <td>short</td>
    </tr>
</table>";

            var expected =
                "short   short                           Lorem ipsum dolor sit amet,\n" +
                "                                        consectetur adipiscing elit,\n" +
                "                                        sed do eiusmod tempor\n" +
                "                                        incididunt ut labore et dolore\n" +
                "                                        magna aliqua. Ut enim ad minim\n" +
                "                                        veniam, quis nostrud\n" +
                "                                        exercitation ullamco laboris\n" +
                "                                        nisi ut aliquip ex ea commodo\n" +
                "                                        consequat.\n" +
                "short   Duis aute irure dolor in        short\n" +
                "        reprehenderit in voluptate\n" +
                "        velit esse cillum dolore eu\n" +
                "        fugiat nulla pariatur.\n" +
                "        Excepteur sint occaecat\n" +
                "        cupidatat non proident, sunt\n" +
                "        in culpa qui officia deserunt\n" +
                "        mollit anim id est laborum.";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";
            options.Table.Options.MaxColumnWidth = 30;
        
            HtmlToText(html, options).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldNotMissContentInTablesWithVariableNumberOfCellsPerRow()
        {
            var html = @"
<table>
    <tr><td>a</td></tr>
    <tr><td>b</td><td>c</td></tr>
    <tr></tr>
    <tr><td>d</td></tr>
</table>";

            var expected = "a\nb   c\n\nd";

            var options = new HtmlToTextOptions();
            options.Table.Format = "dataTable";

            HtmlToText(html, options).ShouldBe(expected);
        }
    }    
    
}