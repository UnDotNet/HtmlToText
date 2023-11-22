# UnDotNet.HtmlToText

HTML to Text converter for DotNet

Ported from [node-html-to-text](https://github.com/html-to-text/node-html-to-text)

Advanced converter that parses HTML and returns beautiful text.

## Features

* Inline and block-level tags.
* Tables with colspans and rowspans.
* Links with both text and href.
* Word wrapping.
* Unicode support.
* Plenty of customization options.

## Installation

Add NuGet package `UnDotNet.HtmlToText` to your project

## Basic Usage

```csharp
using UnDotNet.HtmlToText;

var text = new HtmlToTextConverter().Convert(html);
```

## With Options

```csharp
using UnDotNet.HtmlToText;

var options = new HtmlToTextOptions { 
  wordwrap: 120,
  // ...
};
var text = new HtmlToTextConverter().Convert(html, options);
```

## Example

* Input text: [test.html](test/testassets/test.html)
* Output text: [test.txt](test/testassets/test.txt)

#### General Options

| Option                            | Default             | Description                                                                                                                                                                                                                                                                                            |
|-----------------------------------|---------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `BaseElements`                    |                     | Describes which parts of the input document have to be converted and present in the output text, and in what order.                                                                                                                                                                                    |
| `BaseElements.selectors`          | `['body']`          | Elements matching any of provided selectors will be processed and included in the output text, with all inner content.<br/>Refer to [Supported selectors](#supported-selectors) section below.                                                                                                         |
| `BaseElements.orderBy`            | `'selectors'`       | `'selectors'` - arrange base elements in the same order as `baseElements.selectors` array;<br/>`'occurrence'` - arrange base elements in the order they are found in the input document.                                                                                                               |
| `BaseElements.returnDomByDefault` | `true`              | Convert the entire document if none of provided selectors match.                                                                                                                                                                                                                                       |
| `encodeCharacters`                | `{}`                | A dictionary with characters that should be replaced in the output text and corresponding escape sequences.                                                                                                                                                                                            |
| `formatters`                      | `{}`                | An object with custom formatting functions for specific elements (see [Override formatting](#override-formatting) section below).                                                                                                                                                                      |
| `limits`                          |                     | Describes how to limit the output text in case of large HTML documents.                                                                                                                                                                                                                                |
| `limits.ellipsis`                 | `'...'`             | A string to insert in place of skipped content.                                                                                                                                                                                                                                                        |
| `limits.maxBaseElements`          | `undefined`         | Stop looking for more base elements after reaching this amount. Unlimited if undefined.                                                                                                                                                                                                                |
| `limits.maxChildNodes`            | `undefined`         | Maximum number of child nodes of a single node to be added to the output. Unlimited if undefined.                                                                                                                                                                                                      |
| `limits.maxDepth`                 | `undefined`         | Stop looking for nodes to add to the output below this depth in the DOM tree. Unlimited if undefined.                                                                                                                                                                                                  |
| `limits.maxInputLength`           | `16_777_216`        | If the input string is longer than this value - it will be truncated and a message will be sent to `stderr`. Ellipsis is not used in this case. Unlimited if undefined.                                                                                                                                |
| `longWordSplit`                   |                     | Describes how to wrap long words.                                                                                                                                                                                                                                                                      |
| `longWordSplit.wrapCharacters`    | `[]`                | An array containing the characters that may be wrapped on. Checked in order, search stops once line length requirement can be met.                                                                                                                                                                     |
| `longWordSplit.forceWrapOnLimit`  | `false`             | Break long words at the line length limit in case no better wrap opportunities found.                                                                                                                                                                                                                  |
| `preserveNewlines`                | `false`             | By default, any newlines `\n` from the input HTML are collapsed into space as any other HTML whitespace characters. If `true`, these newlines will be preserved in the output. This is only useful when input HTML carries some plain text formatting instead of proper tags.                          |
| `selectors`                       | `[]`                | Describes how different HTML elements should be formatted. See [Selectors](#selectors) section below.                                                                                                                                                                                                  |
| `whitespaceCharacters`            | `' \t\r\n\f\u200b'` | A string of characters that are recognized as HTML whitespace. Default value uses the set of characters defined in [HTML4 standard](https://www.w3.org/TR/html4/struct/text.html#h-9.1). (It includes Zero-width space compared to [living standard](https://infra.spec.whatwg.org#ascii-whitespace).) |
| `wordwrap`                        | `80`                | After how many chars a line break should follow.<br/>Set to `null` or `false` to disable word-wrapping.                                                                                                                                                                                                |

##### Predefined formatters

Following selectors have a formatter specified as a part of the default configuration. Everything can be overridden, but you don't have to repeat the `format` or options that you don't want to override. (But keep in mind this is only true for the same selector. There is no connection between different selectors.)

| Selector     | Default&nbsp;format | Notes                                                            |
|--------------|---------------------|------------------------------------------------------------------|
| `*`          | `inline`            | Universal selector.                                              |
| `a`          | `anchor`            |                                                                  |
| `article`    | `block`             |                                                                  |
| `aside`      | `block`             |                                                                  |
| `blockquote` | `blockquote`        |                                                                  |
| `br`         | `lineBreak`         |                                                                  |
| `div`        | `block`             |                                                                  |
| `footer`     | `block`             |                                                                  |
| `form`       | `block`             |                                                                  |
| `h1`         | `heading`           |                                                                  |
| `h2`         | `heading`           |                                                                  |
| `h3`         | `heading`           |                                                                  |
| `h4`         | `heading`           |                                                                  |
| `h5`         | `heading`           |                                                                  |
| `h6`         | `heading`           |                                                                  |
| `header`     | `block`             |                                                                  |
| `hr`         | `horizontalLine`    |                                                                  |
| `img`        | `image`             |                                                                  |
| `main`       | `block`             |                                                                  |
| `nav`        | `block`             |                                                                  |
| `ol`         | `orderedList`       |                                                                  |
| `p`          | `paragraph`         |                                                                  |
| `pre`        | `pre`               |                                                                  |
| `table`      | `table`             | Equivalent to `block`. Use `dataTable` instead for tabular data. |
| `ul`         | `unorderedList`     |                                                                  |
| `wbr`        | `wbr`               |                                                                  |

More formatters also available for use:

Format           | Description
---------------- | -----------
`dataTable`      | For visually-accurate tables. Note that this might be not search-friendly (output text will look like gibberish to a machine when there is any wrapped cell contents) and also better to be avoided for tables used as a page layout tool.
`skip`           | Skips the given tag with it's contents without printing anything.
`blockString`    | Insert a block with the given string literal (`formatOptions.string`) instead of the tag.
`blockTag`       | Render an element as HTML block tag, convert it's contents to text.
`blockHtml`      | Render an element with all it's children as HTML block.
`inlineString`   | Insert the given string literal (`formatOptions.string`) inline instead of the tag.
`inlineSurround` | Render inline element wrapped with given strings (`formatOptions.prefix` and `formatOptions.suffix`).
`inlineTag`      | Render an element as inline HTML tag, convert it's contents to text.
`inlineHtml`     | Render an element with all it's children as inline HTML.

##### Format options

Following options are available for built-in formatters.

Option              | Default     | Applies&nbsp;to    | Description
------------------- | ----------- | ------------------ | -----------
`leadingLineBreaks` | `1`, `2` or `3` | all block-level formatters | Number of line breaks to separate previous block from this one.<br/>Note that N+1 line breaks are needed to make N empty lines.
`trailingLineBreaks` | `1` or `2` | all block-level formatters | Number of line breaks to separate this block from the next one.<br/>Note that N+1 line breaks are needed to make N empty lines.
`baseUrl`           | `null`      | `anchor`, `image`  | Server host for link `href` attributes and image `src` attributes relative to the root (the ones that start with `/`).<br/>For example, with `baseUrl = 'http://asdf.com'` and `<a href='/dir/subdir'>...</a>` the link in the text will be `http://asdf.com/dir/subdir`.
`linkBrackets`     | `['[', ']']` | `anchor`, `image`  | Surround links with these brackets.<br/>Set to `false` or `['', '']` to disable.
`pathRewrite`      | `undefined`  | `anchor`, `image`  | A function to rewrite link `href` attributes and image `src` attributes. Optional second argument is the metadata object.<br/>Applied before `baseUrl`.
`hideLinkHrefIfSameAsText` | `false` | `anchor`        | By default links are translated in the following way:<br/>`<a href='link'>text</a>` => becomes => `text [link]`.<br/>If this option is set to `true` and `link` and `text` are the same, `[link]` will be omitted and only `text` will be present.
`ignoreHref`        | `false`     | `anchor`           | Ignore all links. Only process internal text of anchor tags.
`noAnchorUrl`       | `true`      | `anchor`           | Ignore anchor links (where `href='#...'`).
`itemPrefix`        | `' * '`     | `unorderedList`    | String prefix for each list item.
`uppercase`         | `true`      | `heading`          | By default, headings (`<h1>`, `<h2>`, etc) are uppercased.<br/>Set this to `false` to leave headings as they are.
`length`            | `undefined` | `horizontalLine`   | Length of the line. If undefined then `wordwrap` value is used. Falls back to 40 if that's also disabled.
`trimEmptyLines`    | `true`      | `blockquote`       | Trim empty lines from blockquote.<br/>While empty lines should be preserved in HTML, space-saving behavior is chosen as default for convenience.
`uppercaseHeaderCells` | `true`   | `dataTable`        | By default, heading cells (`<th>`) are uppercased.<br/>Set this to `false` to leave heading cells as they are.
`maxColumnWidth`    | `60`        | `dataTable`        | Data table cell content will be wrapped to fit this width instead of global `wordwrap` limit.<br/>Set this to `undefined` in order to fall back to `wordwrap` limit.
`colSpacing`        | `3`         | `dataTable`        | Number of spaces between data table columns.
`rowSpacing`        | `0`         | `dataTable`        | Number of empty lines between data table rows.
`string`            | `''`        | `blockString`, `inlineString` | A string to be inserted in place of a tag.
`prefix`            | `''`        | `inlineSurround`   | String prefix to be inserted before inline tag contents.
`suffix`            | `''`        | `inlineSurround`   | String suffix to be inserted after inline tag contents.
