// ReSharper disable UnusedParameter.Local

using AngleSharp.Dom;

namespace UnDotNet.HtmlToText;

internal static class GenericFormatters
{
  static GenericFormatters()
  {
    Build();
  }
  
  public static Dictionary<string, FormatCallback> Formatters { get; set; } = new();

  private static string RenderOpenTag(IElement elem)
  {

    var attrs = "";
    foreach (var attribute in elem.Attributes)
    {
      if (!string.IsNullOrEmpty(attribute.Value))
      {
        var value = attribute.Value.Replace("\"", "&quot;");
        attrs += $" {attribute.Name}=\"{value}\"";
          
      }
      else
      {
        attrs += $" {attribute.Name}";
      }
    }
    return $"<{elem.TagName} {attrs}>";
  }
  
  private static string RenderCloseTag(IElement elem)
  {
    return $"</{elem.TagName}>";
  }

  private static void Build()
  {
    //
    // Dummy formatter that discards the input and does nothing.
    //
    Formatters.Add("skip", (elem, walk, builder, formatOptions) =>
    {
      // do nothing
    });
    
    //
    // Insert the given string literal inline instead of a tag.
    //
    Formatters.Add("inlineString", (elem, walk, builder, formatOptions) =>
    {
      builder.AddLiteral(formatOptions.StringLiteral);
    });

    //
    // Insert a block with the given string literal instead of a tag. 
    //
    Formatters.Add("blockString", (elem, walk, builder, formatOptions) =>
    {
      builder.OpenBlock(leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 2 );
      builder.AddLiteral(formatOptions.StringLiteral);
      builder.CloseBlock(trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 2 );
    });

    //
    // Process an inline-level element.
    //
    Formatters.Add("inline", (elem, walk, builder, formatOptions) =>
    {
      walk(walk, elem.ChildNodes, builder);
    });
    
    //
    // Process a block-level container.
    //
    Formatters.Add("block", (elem, walk, builder, formatOptions) =>
    {
      builder.OpenBlock(leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 2);
      walk(walk, elem.ChildNodes, builder);
      builder.CloseBlock(trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 2);
    });
    
    //
    // Render an element as inline HTML tag, walk through it's children. 
    //
    Formatters.Add("inlineTag", (elem, walk, builder, formatOptions) =>
    {
      builder.StartNoWrap();
      builder.AddLiteral(RenderOpenTag(elem));
      builder.StopNoWrap();
      walk(walk, elem.ChildNodes, builder);
      builder.StartNoWrap();
      builder.AddLiteral(RenderCloseTag(elem));
      builder.StopNoWrap();
    });
    
    //
    // Render an element as HTML block bag, walk through it's children.
    //
    Formatters.Add("blockTag", (elem, walk, builder, formatOptions) =>
    {
      builder.OpenBlock(leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 2);
      builder.StartNoWrap();
      builder.AddLiteral(RenderOpenTag(elem));
      builder.StopNoWrap();
      walk(walk, elem.ChildNodes, builder);
      builder.StartNoWrap();
      builder.AddLiteral(RenderCloseTag(elem));
      builder.StopNoWrap();
      builder.CloseBlock(trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 2);
    });
    
    //
    // Render an element with all it's children as inline HTML.
    //
    Formatters.Add("inlineHtml", (elem, walk, builder, formatOptions) =>
    {
      builder.StartNoWrap();
      builder.AddLiteral(elem.OuterHtml);
      builder.StopNoWrap();
    });
    
    //
    // Render an element with all it's children as HTML block.
    //
    Formatters.Add("blockHtml", (elem, walk, builder, formatOptions) =>
    {
      builder.OpenBlock(leadingLineBreaks: formatOptions.LeadingLineBreaks ?? 2);
      builder.StartNoWrap();
      builder.AddLiteral(elem.OuterHtml);
      builder.StopNoWrap();
      builder.CloseBlock(trailingLineBreaks: formatOptions.TrailingLineBreaks ?? 2);
    });

    //
    // Render inline element wrapped with given strings.
    //
    Formatters.Add("inlineSurround", (elem, walk, builder, formatOptions) =>
    {
      builder.AddLiteral(formatOptions.Prefix);
      walk(walk, elem.ChildNodes, builder);
      builder.AddLiteral(formatOptions.Suffix);
    });

  }

}