using System.Diagnostics;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace UnDotNet.HtmlToText;

public class HtmlToTextConverter
{
    
    public string Convert(string? html = null, HtmlToTextOptions? opts = null, Dictionary<string, string>? metaData = null)
    {
        if (html is null) return "";
        opts ??= new();
        
        var maxInputLength = opts.limits.maxInputLength;
        if (maxInputLength > 0 && html.Length > maxInputLength)
        {
            Trace.WriteLine($"Input length {html.Length} is above allowed limit of {maxInputLength}. Truncating without ellipsis.");
            html = html.Substring(0, maxInputLength);
        }
        
        opts.formatters = DictionaryExtensions.MergeValues<string, FormatCallback>(null, GenericFormatters.Formatters, TextFormatters.Formatters, opts.formatters);
        
        // selectors
        CssSelectorParser cssparser = new();

        Dictionary<ISelector, Selector> rules = new();
        
        foreach (var selector in opts.selectors)
        {
            var cssselector = cssparser.ParseSelector(selector.selector);
            if (cssselector is not null)
            {
                rules.Add(cssselector, selector);
            }
        }
        
        var builder = new BlockTextBuilder(opts, metaData);
        builder.CssRules = rules;

        var parser = new HtmlParser();
        var doc = parser.ParseDocument(html);

        if (doc.Body.ChildNodes.Length == 0)
        {
            var p = new HtmlParser();
            doc = p.ParseDocument($"<html><body>{html}</body></html>");
        }

        maxDepth = opts.limits?.maxDepth ?? int.MaxValue;

        if (opts.BaseElements.selectors.Count > 0)
        {
            if (opts.BaseElements.orderBy is not "selectors")
            {
                var allBases = opts.BaseElements.selectors.ToArray().Join(", ");
                var currentBase = doc.QuerySelectorAll(allBases).Take(opts.limits?.maxBaseElements ?? 9999);
                var enumerable = currentBase as IElement[] ?? currentBase.ToArray();
                if (enumerable.Any())
                {
                    // need to offset for non-body elements
                    maxDepth += 1;
                    RecursiveWalk(RecursiveWalk, enumerable, builder);
                    return builder.ToString();
                }
            }
            else
            {
                var allBases = new List<IElement>(); 
                foreach (var elementsSelector in opts.BaseElements.selectors)
                {
                    var currentBase = doc.QuerySelectorAll(elementsSelector).ToArray();
                    foreach (var testBase in currentBase)
                    {
                        if (!allBases.Contains(testBase))
                        {
                            allBases.Add(testBase);
                        }
                        
                    }
                    // allBases.AddRange(currentBase);
                }
                if (allBases.Any())
                {
                    var limited = allBases.Take(opts.limits?.maxBaseElements ?? 9999);
                    // need to offset for non-body elements
                    maxDepth += 1;
                    RecursiveWalk(RecursiveWalk, limited, builder);
                    return builder.ToString();
                }
            }
        }
        
        if (opts.BaseElements.returnDomByDefault)
        {
            RecursiveWalk(RecursiveWalk, doc.Body.ChildNodes, builder);
        }

        return builder.ToString();

    }
    //
    // public int Limited(int? allowedDepth, RecursiveCallback callback, IEnumerable<IElement> dom, BlockTextBuilder builder)
    // {
    //     if (allowedDepth is null) callback(callback, dom, builder);
    //     if (allowedDepth is >= 0)
    //     {
    //         callback(callback, dom, builder);
    //         return Limited(allowedDepth - 1, callback, dom, builder);
    //         
    //         return Limited(allowedDepth = 1, callback, dom, builder)
    //     }
    //     
    // }
    //
    // public static Func<T, TResult> LimitedDepthRecursivea<T, TResult>(int n, Func<Func<T, TResult>, T, TResult> f, Func<TResult> g = null)
    // {
    //     if (n == 0)
    //     {
    //         Func<T, TResult> f1 = (args) => f(f1, args);
    //         return f1;
    //     }
    //     if (n > 0)
    //     {
    //         return (args) => f(LimitedDepthRecursive(n - 1, f, g), args);
    //     }
    //     return g;
    // }

    private int maxDepth = 9999;
    
    private void RecursiveWalk(RecursiveCallback walk, IEnumerable<INode> dom, BlockTextBuilder builder)
    {
        maxDepth -= 1;

        if (maxDepth < 0)
        { 
            builder.addInline(builder.Options.limits.ellipsis ?? "");
            return;
        }
        
        if (dom == null) { return; }
        var options = builder.Options;
        var tooManyChildNodes = dom.Count() > (options.limits.maxChildNodes ?? int.MaxValue);
        if (tooManyChildNodes)
        {
            dom = dom.Take((options.limits.maxChildNodes ?? int.MaxValue)).ToArray();
        }
        foreach (var elem in dom)
        {
            switch (elem.NodeType)
            {
                case NodeType.Text:
                    builder.addInline(elem.NodeValue);
                    break;
                case NodeType.Element:
                {
                    if (elem is IHtmlElement el)
                    {
                        if (el.NodeName == "BODY")
                        {
                            walk(walk, el.ChildNodes, builder);
                            continue;
                        }
                        // var key = getKey(el, options);
                        // var selector = options.selectors[key];

                        var selector = builder.PickSelector(el);
                        
                        if (selector is not null)
                        {
                            var format = options.formatters[selector.format];
                            format(el, walk, builder, selector.options);
                        }
                    } 

                    break;
                }
                /* do nothing */
                default:
                    break;
            }
        }

        if (tooManyChildNodes)
        {
            builder.addInline(options.limits.ellipsis);
        }
    }
    
}