namespace UnDotNet.HtmlToText;

public class TablePrinterCell
{
    public int Rowspan { get; private set; }
    public int Colspan { get; private set; }
    public string Text { get; private set; } = "";
    public string[] Lines { get; set; }
    public bool Rendered { get; set; }

    public TablePrinterCell(int rowspan, int colspan, string text)
    {
        Rowspan = rowspan;
        Colspan = colspan;
        Text = text;
    }
}


internal static class TablePrinterUtils
{
    private static List<TablePrinterCell?> GetRow(List<List<TablePrinterCell?>> matrix, int j)
    {
        if (matrix.Count <= j)
        {
            matrix.Add(new List<TablePrinterCell?>());
        }
        return matrix[j];
    }

    private static int FindFirstVacantIndex(List<TablePrinterCell?> row, int x = 0)
    {
        while (row.Count > x && row[x] != null)
        {
            x++;
        }
        return x;
    }

    // public static TablePrinterCell? getCol(List<TablePrinterCell> row, int colIndex)
    // {
    //     if ()
    //     return null;
    // }

    private static void TransposeInPlace(List<List<TablePrinterCell?>> matrix, int maxSize)
    {
        for (var i = 0; i < maxSize; i++)
        {
            var rowI = GetRow(matrix, i);
            for (var j = 0; j < i; j++)
            {
                var rowJ = GetRow(matrix, j);

                if (rowI.Count - 1 < j)
                {
                    for (var k = rowI.Count - 1; k < j; k++)
                    {
                        rowI.Add(null);
                    }
                }

                if (rowJ.Count - 1 < i)
                {
                    for (var k = rowJ.Count - 1; k < i; k++)
                    {
                        rowJ.Add(null);
                    }
                }

                
                // index = 1, expects 2 rows
                // count of rows = 1
                // index = 2, expects 3 rows
                // count of rows = 1 

                
                
                if (rowI[j] != null || rowJ[i] != null)
                {
                    (rowI[j], rowJ[i]) = (rowJ[i], rowI[j]);
                }
            }
        }
    }

    private static void PutCellIntoLayout(TablePrinterCell cell, List<List<TablePrinterCell>> layout, int baseRow, int baseCol)
    {
        for (var r = 0; r < cell.Rowspan; r++)
        {
            var layoutRow = GetRow(layout, baseRow + r);
            for (var c = 0; c < cell.Colspan; c++)
            {
                if (layoutRow.Count - 1 <= baseCol + c)
                {
                    for (var k = layoutRow.Count - 1; k < baseCol + c; k++)
                    {
                        layoutRow.Add(null);
                    }
                }
                layoutRow[baseCol + c] = cell;
            }
        }
    }

    private static int GetOrInitOffset(List<int> offsets, int index)
    {
        if (offsets.Count <= index)
        {
            offsets.Add(index == 0 ? 0 : 1 + GetOrInitOffset(offsets, index - 1));
        }
        return offsets[index];
    }

    private static void UpdateOffset(List<int> offsets, int @base, int span, int value)
    {
        offsets[@base + span] = Math.Max(
            GetOrInitOffset(offsets, @base + span),
            GetOrInitOffset(offsets, @base) + value
        );
    }

    public static string TableToString(List<List<TablePrinterCell>> tableRows, int rowSpacing, int colSpacing)
    {
        var layout = new List<List<TablePrinterCell>>();
        var colNumber = 0;
        var rowNumber = tableRows.Count;
        var rowOffsets = new List<int> { 0 };

        for (var j = 0; j < rowNumber; j++)
        {
            var layoutRow = GetRow(layout, j);
            var cells = tableRows[j];
            var x = 0;
            foreach (var cell in cells)
            {
                x = FindFirstVacantIndex(layoutRow, x);
                PutCellIntoLayout(cell, layout, j, x);
                x += cell.Colspan;
                cell.Lines = cell.Text.Split('\n');
                var cellHeight = cell.Lines.Length;
                UpdateOffset(rowOffsets, j, cell.Rowspan, cellHeight + rowSpacing);
            }
            colNumber = layoutRow.Count > colNumber ? layoutRow.Count : colNumber;
        }

        // ensure rows have all null columns needed
        // foreach (var row in layout)
        // {
        //     if (row.Count < colNumber)
        //     {
        //         for (int i = row.Count; i < colNumber; i++)
        //         {
        //             row.Add(null);
        //         }
        //     }
        //     
        // }
        
        TransposeInPlace(layout, rowNumber > colNumber ? rowNumber : colNumber);
        var outputLines = new List<string>();
        var colOffsets = new List<int>() { 0 };

        for (var x = 0; x < colNumber; x++)
        {
            var y = 0;
            var rowsInThisColumn = Math.Min(rowNumber, layout[x].Count);
            while (y < rowsInThisColumn)
            {
                var cell = layout[x][y];
                if (cell != null)
                {
                    if (!cell.Rendered)
                    {
                        var cellWidth = 0;
                        if (cell.Lines != null)
                            for (var j = 0; j < cell.Lines.Length; j++)
                            {
                                var line = cell.Lines[j];
                                var lineOffset = rowOffsets[y] + j;
                                if (outputLines.Count - 1 < lineOffset)
                                {
                                    for (var k = outputLines.Count - 1; k < lineOffset; k++)
                                    {
                                        outputLines.Add(null);
                                    }
                                }

                                var colOffset = 0;
                                if (colOffsets.Count > x)
                                {
                                    colOffset = GetOrInitOffset(colOffsets, x);
                                }

                                outputLines[lineOffset] = (outputLines[lineOffset] ?? "").PadRight(colOffset) + line;

                                cellWidth = line.Length > cellWidth ? line.Length : cellWidth;
                            }

                        UpdateOffset(colOffsets, x, cell.Colspan, cellWidth + colSpacing);
                        cell.Rendered = true;
                    }
                    y += cell.Rowspan;
                }
                else
                {
                    var lineOffset = rowOffsets[y];
                    
                    if (outputLines.Count - 1 < lineOffset)
                    {
                        for (var k = outputLines.Count - 1; k < lineOffset; k++)
                        {
                            outputLines.Add(null);
                        }
                    }
                    
                    outputLines[lineOffset] ??= "";
                    y++;
                }
            }
        }
        return string.Join("\n", outputLines);
    }
}