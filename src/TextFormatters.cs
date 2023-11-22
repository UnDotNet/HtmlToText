using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace UnDotNet.HtmlToText;

public class TextFormatters
{
  static TextFormatters()
  {
    Build();
  }

  public static Dictionary<string, FormatCallback> Formatters { get; set; } = new();
  
  static string withBrackets(string str, BracketOptions? linkBrackets)
  {
    linkBrackets ??= new () {left = "", right = ""};
    return linkBrackets.left + str + linkBrackets.right;
  }

  // path, rewriter, baseUrl, metadata, elem
  static string pathRewrite(string path, RewriterCallback? rewriter = null, string? baseUrl = null, Dictionary<string, string>? metadata = null, IElement? elem = null)
  {
    string modifiedPath = rewriter is not null ? rewriter(path, metadata, elem) : path;
    return (modifiedPath[0] == '/' && baseUrl != null)
      ? baseUrl.TrimEnd('/') + modifiedPath
      : modifiedPath;
  }
  
  /**
* @param { DomNode }           elem               List items with their prefixes.
* @param { RecursiveCallback } walk               Recursive callback to process child nodes.
* @param { BlockTextBuilder }  builder            Passed around to accumulate output text.
* @param { FormatOptions }     formatOptions      Options specific to a formatter.
* @param { () => string }      nextPrefixCallback Function that returns increasing index each time it is called.
*/
  static void formatList(IElement elem, RecursiveCallback walk, BlockTextBuilder builder, FormatOptions formatOptions, Func<string> nextPrefixCallback)
  {
    var isNestedList = elem.ParentElement?.NodeName is "LI";
    
    // With Roman numbers, index length is not as straightforward as with Arabic numbers or letters,
    // so the dumb length comparison is the most robust way to get the correct value.
    int maxPrefixLength = 0;
    if (elem.Children.Length == 0) return;
    IEnumerable<(INode node, string prefix)> listItems = (elem.ChildNodes)
      .Where(child => child.NodeType != NodeType.Text || !Regex.IsMatch(child.NodeValue, @"^\s*$"))
      .Select(child =>
      {
        if (child.NodeName != "LI")
        {
          return (node: child, prefix: "");
        }
        string prefix = isNestedList ? nextPrefixCallback().TrimStart() : nextPrefixCallback();
        if (prefix.Length > maxPrefixLength)
        {
          maxPrefixLength = prefix.Length;
        }
        return (node: child, prefix: prefix);
      })
      .ToList();
    
    if (!listItems.Any()) return;

    builder.openList(
      interRowLineBreaks: 1,
      leadingLineBreaks: isNestedList ? 1 : formatOptions.leadingLineBreaks ?? 2,
      maxPrefixLength: maxPrefixLength,
      prefixAlign: "left"
    );
    
    foreach (var (node, prefix) in listItems)
    {
      builder.openListItem(prefix: prefix);
      walk(walk, new [] {node}, builder);
      builder.closeListItem();
    }
    
    builder.closeList(trailingLineBreaks: isNestedList ? 1 : formatOptions.trailingLineBreaks ?? 2);
  }
  
  //
  // Given a list of class and ID selectors (prefixed with '.' and '#'),
  // return them as separate lists of names without prefixes.
  //
  public static (List<string> classes, List<string> ids) SplitClassesAndIds(string[] selectors)
  {
    var classes = new List<string>();
    var ids = new List<string>();
    foreach (string selector in selectors)
    {
      if (selector.StartsWith("."))
      {
        classes.Add(selector.Substring(1));
      }
      else if (selector.StartsWith("#"))
      {
        ids.Add(selector.Substring(1));
      }
    }
    return (classes, ids);
  }
  
  //
  // Return a function that can be used to generate index markers of a specified format.
  //
  public static Func<int, string> getOrderedListIndexFunction(string olType = "1")
  {
    switch (olType)
    {
      case "a":
        return (i) => Utils.NumberToLetterSequence(i, 'a');
      case "A":
        return (i) => Utils.NumberToLetterSequence(i, 'A');
      case "i":
        return (i) => Utils.NumberToRoman(i).ToLower();
      case "I":
        return (i) => Utils.NumberToRoman(i);
      case "1":
      default:
        return (i) => i.ToString();
    }
  }
  
  public static void Build()
  {

    //
    // Process an anchor. 
    //
    Formatters.Add("anchor", (elem, walk, builder, formatOptions) =>
    {
      string GetHref()
      {
        if (formatOptions.ignoreHref) { return ""; }
        var href = elem.Attributes["href"]?.Value;
        if (string.IsNullOrEmpty(href)) return "";
        href = href.Replace("mailto:", "");
        if (formatOptions.noAnchorUrl && href[0] == '#') return "";
        // path, rewriter, baseUrl, metadata, elem
        href = pathRewrite(href, formatOptions.pathRewrite, formatOptions.baseUrl, builder.metaData,  elem);
        return href;
      }
      var href = GetHref();
      if (string.IsNullOrEmpty(href))
      {
        walk(walk, elem.ChildNodes, builder);
      }
      else
      {
        var text = "";
        builder.pushWordTransform(
          str => {
            if (!string.IsNullOrEmpty(str)) { text += str; }
            return str;
          }
        );
        walk(walk, elem.ChildNodes, builder);
        builder.popWordTransform();
        
        var hideSameLink = formatOptions.hideLinkHrefIfSameAsText && href == text;
        
        if (!hideSameLink) {
          builder.addInline(
            string.IsNullOrEmpty(text)
              ? href
              : ' ' + withBrackets(href, formatOptions.linkBrackets),
            noWordTransform: true
          );
        }
      }
    });
    
    //
    // Process a blockquote. 
    //
    Formatters.Add("blockquote", (elem, walk, builder, formatOptions) =>
    {
      builder.openBlock(
        leadingLineBreaks: formatOptions.leadingLineBreaks ?? 2,
        reservedLineLength: 2
      );
      walk(walk, elem.ChildNodes, builder);
      builder.closeBlock(
        trailingLineBreaks: formatOptions.trailingLineBreaks ?? 2,
        blockTransform: str => (formatOptions.trimEmptyLines ? str.Trim('\n') : str)
          .Split('\n')
          .Select(line => "> " + line).ToArray()
          .Join('\n'));
    });
    
    //
    // Process a data table. 
    //
    Formatters.Add("dataTable", (elem, walk, builder, formatOptions) =>
    {
      builder.openTable();
      foreach (var child in elem.ChildNodes) walkTable(child);
      
      builder.closeTable(
        tableToString: (rows, rowSpacing, colSpacing) => TablePrinterUtils.tableToString(rows, formatOptions.rowSpacing ?? 0, formatOptions.colSpacing ?? 3),
        leadingLineBreaks: formatOptions.leadingLineBreaks ?? 2,
        trailingLineBreaks: formatOptions.trailingLineBreaks ?? 2
      );
  
      void formatCell (IElement cellNode)
      {
        int colspan = 1;
        if (int.TryParse(cellNode.Attributes["colspan"]?.Value ?? "1", out var testColspan))
        {
          colspan = testColspan;
        }
        int rowspan = 1;
        if (int.TryParse(cellNode.Attributes["rowspan"]?.Value ?? "1", out var testRowspan))
        {
          rowspan = testRowspan;
        }
        builder.openTableCell(maxColumnWidth: formatOptions.maxColumnWidth);
        walk(walk, cellNode.ChildNodes, builder);
        builder.closeTableCell(colspan: colspan, rowspan: rowspan);
      }
      void walkTable (INode elem) {
        if (elem.NodeType != NodeType.Element) { return; }

        var htmlElement = elem as IElement;
        
        Action<IElement> formatHeaderCell = (formatOptions.uppercaseHeaderCells != false)
          ? (IElement cellNode) => {
            builder.pushWordTransform(str => str.ToUpper());
            formatCell(cellNode);
            builder.popWordTransform();
          }
          : formatCell;
        
        switch (htmlElement.TagName.ToLower()) {
          case "thead":
          case "tbody":
          case "tfoot":
          case "center":
            foreach (var child in elem.ChildNodes) walkTable(child);
            return;
          case "tr": {
            builder.openTableRow();
            foreach (var childOfTr in elem.ChildNodes) {
              if (childOfTr.NodeType != NodeType.Element) { continue; }
              var childElement = childOfTr as IElement;
              

              switch (childElement.TagName.ToLower())
              {
                case "th":
                {
                  formatHeaderCell(childElement);
                  break;
                }
                case "td":
                {
                  formatCell(childElement);
                  break;
                }
                default:
                  break;
                // do nothing
              }
            }
            builder.closeTableRow();
            break;
          }
          default:
            break;
          // do nothing
        }
      }


      
      
      
    });
    
    //
    // Process a heading. 
    //
    Formatters.Add("heading", (elem, walk, builder, formatOptions) =>
    {
      builder.openBlock(leadingLineBreaks: formatOptions.leadingLineBreaks ?? 2);
      if (formatOptions.uppercase) {
        builder.pushWordTransform(str => str.ToUpperInvariant());
        walk(walk, elem.ChildNodes, builder);
        builder.popWordTransform();
      } else {
        walk(walk, elem.ChildNodes, builder);
      }
      builder.closeBlock(trailingLineBreaks: formatOptions.trailingLineBreaks ?? 2);
    });

    //
    // Process a horizontal line.
    //
    Formatters.Add("horizontalLine", (elem, walk, builder, formatOptions) =>
    {
      var length = 40;
      if (formatOptions.length is not null)
      {
        length = formatOptions.length.Value;
      }
      else if (builder.Options.wordwrap is > 0)
      {
        length = builder.Options.wordwrap.Value;
      }
      
      builder.openBlock(leadingLineBreaks: formatOptions.leadingLineBreaks ?? 2);
      builder.addInline("-".Repeat(length));
      builder.closeBlock(trailingLineBreaks: formatOptions.trailingLineBreaks ?? 2);
    });
    
    //
    // Process an image.
    //
    Formatters.Add("image", (elem, walk, builder, formatOptions) =>
    {
      var alt = elem.Attributes["alt"]?.Value ?? "";
      var src = elem.Attributes["src"]?.Value ?? "";
      if (!string.IsNullOrEmpty(src))
      {
        src = pathRewrite(src, formatOptions.pathRewrite, formatOptions.baseUrl, builder.metaData, elem);
      }
      var text = alt;
      if (!string.IsNullOrEmpty(src))
      {
        if (!string.IsNullOrEmpty(text)) text += " ";
        text += withBrackets(src, formatOptions.linkBrackets);
      }
      builder.addInline(text, noWordTransform: true);
    });

    //
    // Process a line-break.
    //
    Formatters.Add("lineBreak", (elem, walk, builder, formatOptions) =>
    {
      builder.addLineBreak();
    });
    
    //
    // Process an ordered list. 
    //
    Formatters.Add("orderedList", (elem, walk, builder, formatOptions) =>
    {
      var start = elem.Attributes["start"]?.Value ?? "1";
      var nextIndex = int.Parse(start);
      var indexFunction = getOrderedListIndexFunction(elem.Attributes["type"]?.Value ?? "1");
      var nextPrefixCallback = () => " " + indexFunction(nextIndex++) + ". ";
      formatList(elem, walk, builder, formatOptions, nextPrefixCallback);
    });

    //
    // Process a paragraph.
    //
    Formatters.Add("paragraph", (elem, walk, builder, formatOptions) =>
    {
      builder.openBlock(leadingLineBreaks: formatOptions.leadingLineBreaks ?? 2);
      walk(walk, elem.ChildNodes, builder);
      builder.closeBlock(trailingLineBreaks: formatOptions.trailingLineBreaks ?? 2);
    });
    
    //
    // Process a preformatted content.
    //
    Formatters.Add("pre", (elem, walk, builder, formatOptions) =>
    {
      builder.openBlock(isPre: true, leadingLineBreaks: formatOptions.leadingLineBreaks ?? 2);
      walk(walk, elem.ChildNodes, builder);
      builder.closeBlock(trailingLineBreaks: formatOptions.trailingLineBreaks ?? 2);
    });

    //
    // 
    //
    Formatters.Add("table", (elem, walk, builder, formatOptions) =>
    {
      builder.openBlock(leadingLineBreaks: formatOptions.leadingLineBreaks ?? 1);
      walk(walk, elem.ChildNodes, builder);
      builder.closeBlock(trailingLineBreaks: formatOptions.trailingLineBreaks ?? 1);
    });
    
    //
    // Process an unordered list.
    //
    Formatters.Add("unorderedList", (elem, walk, builder, formatOptions) =>
    {
      var prefix = formatOptions.itemPrefix ?? " * ";
      formatList(elem, walk, builder, formatOptions, () => prefix);
    });

    //
    // Process a `wbr` tag (word break opportunity).
    //
    Formatters.Add("wbr", (elem, walk, builder, formatOptions) =>
    {
      builder.addWordBreakOpportunity();
    });


  }

}