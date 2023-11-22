using System.Text.RegularExpressions;
using AngleSharp.Dom;
// ReSharper disable UnusedParameter.Local

namespace UnDotNet.HtmlToText;

public static class TextFormatters
{
  static TextFormatters()
  {
    Build();
  }

  public static Dictionary<string, FormatCallback> Formatters { get; set; } = new();
  
  static string WithBrackets(string str, BracketOptions? linkBrackets)
  {
    linkBrackets ??= new () {Left = "", Right = ""};
    return linkBrackets.Left + str + linkBrackets.Right;
  }

  // path, rewriter, baseUrl, metadata, elem
  static string PathRewrite(string path, RewriterCallback? rewriter = null, string? baseUrl = null, Dictionary<string, string>? metadata = null, IElement? elem = null)
  {
    var modifiedPath = rewriter is not null ? rewriter(path, metadata, elem) : path;
    return (modifiedPath != null && modifiedPath[0] == '/' && baseUrl != null
      ? baseUrl.TrimEnd('/') + modifiedPath
      : modifiedPath) ?? string.Empty;
  }
  
  /**
* @param { DomNode }           elem               List items with their prefixes.
* @param { RecursiveCallback } walk               Recursive callback to process child nodes.
* @param { BlockTextBuilder }  builder            Passed around to accumulate output text.
* @param { FormatOptions }     formatOptions      Options specific to a formatter.
* @param { () => string }      nextPrefixCallback Function that returns increasing index each time it is called.
*/
  static void FormatList(IElement elem, RecursiveCallback walk, BlockTextBuilder builder, FormatOptions formatOptions, Func<string> nextPrefixCallback)
  {
    var isNestedList = elem.ParentElement?.NodeName is "LI";
    
    // With Roman numbers, index length is not as straightforward as with Arabic numbers or letters,
    // so the dumb length comparison is the most robust way to get the correct value.
    var maxPrefixLength = 0;
    if (elem.Children.Length == 0) return;
    IEnumerable<(INode node, string prefix)> listItems = (elem.ChildNodes)
      .Where(child => child.NodeType != NodeType.Text || !Regex.IsMatch(child.NodeValue, @"^\s*$"))
      .Select(child =>
      {
        if (child.NodeName != "LI")
        {
          return (node: child, prefix: "");
        }
        var prefix = isNestedList ? nextPrefixCallback().TrimStart() : nextPrefixCallback();
        if (prefix.Length > maxPrefixLength)
        {
          maxPrefixLength = prefix.Length;
        }
        return (node: child, prefix);
      })
      .ToList();
    
    if (!listItems.Any()) return;

    builder.OpenList(
      interRowLineBreaks: 1,
      leadingLineBreaks: isNestedList ? 1 : formatOptions.LeadingLineBreaks ?? 2,
      maxPrefixLength: maxPrefixLength,
      prefixAlign: "left"
    );
    
    foreach (var (node, prefix) in listItems)
    {
      builder.OpenListItem(prefix: prefix);
      walk(walk, new [] {node}, builder);
      builder.CloseListItem();
    }
    
    builder.CloseList(trailingLineBreaks: isNestedList ? 1 : formatOptions.TrailingLineBreaks ?? 2);
  }
  
  //
  // Given a list of class and ID selectors (prefixed with '.' and '#'),
  // return them as separate lists of names without prefixes.
  //
  public static (List<string> classes, List<string> ids) SplitClassesAndIds(string[] selectors)
  {
    var classes = new List<string>();
    var ids = new List<string>();
    foreach (var selector in selectors)
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
  private static Func<int, string> GetOrderedListIndexFunction(string olType = "1")
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
        // ReSharper disable once ConvertClosureToMethodGroup
        return (i) => Utils.NumberToRoman(i);
      default:
        return (i) => i.ToString();
    }
  }

  private static void Build()
  {

    //
    // Process an anchor. 
    //
    Formatters.Add("anchor", (elem, walk, builder, formatOptions) =>
    {
      string GetHref()
      {
        if (formatOptions.IgnoreHref) { return ""; }
        var href = elem.Attributes["href"]?.Value;
        if (string.IsNullOrEmpty(href)) return "";
        href = href.Replace("mailto:", "");
        if (formatOptions.NoAnchorUrl && href[0] == '#') return "";
        // path, rewriter, baseUrl, metadata, elem
        href = PathRewrite(href, formatOptions.PathRewrite, formatOptions.BaseUrl, builder.MetaData,  elem);
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
        builder.PushWordTransform(
          str => {
            if (!string.IsNullOrEmpty(str)) { text += str; }
            return str;
          }
        );
        walk(walk, elem.ChildNodes, builder);
        builder.PopWordTransform();
        
        var hideSameLink = formatOptions.HideLinkHrefIfSameAsText && href == text;
        
        if (!hideSameLink) {
          builder.AddInline(
            string.IsNullOrEmpty(text)
              ? href
              : ' ' + WithBrackets(href, formatOptions.LinkBrackets),
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
      builder.OpenBlock(
        leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 2,
        reservedLineLength: 2
      );
      walk(walk, elem.ChildNodes, builder);
      builder.CloseBlock(
        trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 2,
        blockTransform: str => (formatOptions.TrimEmptyLines ? str.Trim('\n') : str)
          .Split('\n')
          .Select(line => "> " + line).ToArray()
          .Join('\n'));
    });
    
    //
    // Process a data table. 
    //
    Formatters.Add("dataTable", (elem, walk, builder, formatOptions) =>
    {
      builder.OpenTable();
      foreach (var child in elem.ChildNodes) WalkTable(child);
      
      builder.CloseTable(
        tableToString: (rows, rowSpacing, colSpacing) => TablePrinterUtils.TableToString(rows, formatOptions.RowSpacing ?? 0, formatOptions.ColSpacing ?? 3),
        leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 2,
        trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 2
      );
  
      void FormatCell (IElement cellNode)
      {
        var colspan = 1;
        if (int.TryParse(cellNode.Attributes["colspan"]?.Value ?? "1", out var testColspan))
        {
          colspan = testColspan;
        }
        var rowspan = 1;
        if (int.TryParse(cellNode.Attributes["rowspan"]?.Value ?? "1", out var testRowspan))
        {
          rowspan = testRowspan;
        }
        builder.OpenTableCell(maxColumnWidth: formatOptions.MaxColumnWidth);
        walk(walk, cellNode.ChildNodes, builder);
        builder.CloseTableCell(colspan: colspan, rowspan: rowspan);
      }
      
      void WalkTable (INode walkElem) {
        if (walkElem.NodeType != NodeType.Element) { return; }

        var htmlElement = walkElem as IElement;
        
        Action<IElement> formatHeaderCell = formatOptions.UppercaseHeaderCells
          ? cellNode => {
            builder.PushWordTransform(str => str.ToUpper());
            FormatCell(cellNode);
            builder.PopWordTransform();
          }
          : FormatCell;

        if (htmlElement == null) return;
        switch (htmlElement.TagName.ToLower())
        {
          case "thead":
          case "tbody":
          case "tfoot":
          case "center":
            foreach (var child in walkElem.ChildNodes) WalkTable(child);
            return;
          case "tr":
          {
            builder.OpenTableRow();
            foreach (var childOfTr in walkElem.ChildNodes)
            {
              if (childOfTr.NodeType != NodeType.Element)
              {
                continue;
              }

              if (childOfTr is IElement childElement)
                switch (childElement.TagName.ToLower())
                {
                  case "th":
                  {
                    formatHeaderCell(childElement);
                    break;
                  }
                  case "td":
                  {
                    FormatCell(childElement);
                    break;
                  }
                  // do nothing
                }
            }

            builder.CloseTableRow();
            break;
          }
          // do nothing
        }
      }
    });
    
    //
    // Process a heading. 
    //
    Formatters.Add("heading", (elem, walk, builder, formatOptions) =>
    {
      builder.OpenBlock(leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 2);
      if (formatOptions.Uppercase) {
        builder.PushWordTransform(str => str.ToUpperInvariant());
        walk(walk, elem.ChildNodes, builder);
        builder.PopWordTransform();
      } else {
        walk(walk, elem.ChildNodes, builder);
      }
      builder.CloseBlock(trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 2);
    });

    //
    // Process a horizontal line.
    //
    Formatters.Add("horizontalLine", (elem, walk, builder, formatOptions) =>
    {
      var length = 40;
      if (formatOptions.Length is not null)
      {
        length = formatOptions.Length.Value;
      }
      else if (builder.Options.Wordwrap is > 0)
      {
        length = builder.Options.Wordwrap.Value;
      }
      
      builder.OpenBlock(leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 2);
      builder.AddInline("-".Repeat(length));
      builder.CloseBlock(trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 2);
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
        src = PathRewrite(src, formatOptions.PathRewrite, formatOptions.BaseUrl, builder.MetaData, elem);
      }
      var text = alt;
      if (!string.IsNullOrEmpty(src))
      {
        if (!string.IsNullOrEmpty(text)) text += " ";
        text += WithBrackets(src, formatOptions.LinkBrackets);
      }
      builder.AddInline(text, noWordTransform: true);
    });

    //
    // Process a line-break.
    //
    Formatters.Add("lineBreak", (elem, walk, builder, formatOptions) =>
    {
      builder.AddLineBreak();
    });
    
    //
    // Process an ordered list. 
    //
    Formatters.Add("orderedList", (elem, walk, builder, formatOptions) =>
    {
      var start = elem.Attributes["start"]?.Value ?? "1";
      var nextIndex = int.Parse(start);
      var indexFunction = GetOrderedListIndexFunction(elem.Attributes["type"]?.Value ?? "1");
      string NextPrefixCallback() => " " + indexFunction(nextIndex++) + ". ";
      FormatList(elem, walk, builder, formatOptions, NextPrefixCallback);
    });

    //
    // Process a paragraph.
    //
    Formatters.Add("paragraph", (elem, walk, builder, formatOptions) =>
    {
      builder.OpenBlock(leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 2);
      walk(walk, elem.ChildNodes, builder);
      builder.CloseBlock(trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 2);
    });
    
    //
    // Process a preformatted content.
    //
    Formatters.Add("pre", (elem, walk, builder, formatOptions) =>
    {
      builder.OpenBlock(isPre: true, leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 2);
      walk(walk, elem.ChildNodes, builder);
      builder.CloseBlock(trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 2);
    });

    //
    // 
    //
    Formatters.Add("table", (elem, walk, builder, formatOptions) =>
    {
      builder.OpenBlock(leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 1);
      walk(walk, elem.ChildNodes, builder);
      builder.CloseBlock(trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 1);
    });
    
    //
    // Process an unordered list.
    //
    Formatters.Add("unorderedList", (elem, walk, builder, formatOptions) =>
    {
      var prefix = formatOptions.ItemPrefix;
      FormatList(elem, walk, builder, formatOptions, () => prefix);
    });

    //
    // Process a `wbr` tag (word break opportunity).
    //
    Formatters.Add("wbr", (elem, walk, builder, formatOptions) =>
    {
      builder.AddWordBreakOpportunity();
    });


  }

}