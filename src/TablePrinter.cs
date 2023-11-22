namespace UnDotNet.HtmlToText;

public class TablePrinterCell
{
    public int rowspan { get; set; }
    public int colspan { get; set; }
    public string text { get; set; }
    public string[] lines { get; set; }
    public bool rendered { get; set; }
}

internal static class TablePrinterUtils
{
    public static List<TablePrinterCell> getRow(List<List<TablePrinterCell>> matrix, int j)
    {
        if (matrix.Count <= j)
        {
            matrix.Add(new List<TablePrinterCell>());
        }
        return matrix[j];
    }

    public static int findFirstVacantIndex(List<TablePrinterCell> row, int x = 0)
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

    public static void transposeInPlace(List<List<TablePrinterCell>> matrix, int maxSize)
    {
        for (int i = 0; i < maxSize; i++)
        {
            var rowI = getRow(matrix, i);
            for (int j = 0; j < i; j++)
            {
                var rowJ = getRow(matrix, j);

                if (rowI.Count - 1 < j)
                {
                    for (int k = rowI.Count - 1; k < j; k++)
                    {
                        rowI.Add(null);
                    }
                }

                if (rowJ.Count - 1 < i)
                {
                    for (int k = rowJ.Count - 1; k < i; k++)
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

    public static void putCellIntoLayout(TablePrinterCell cell, List<List<TablePrinterCell>> layout, int baseRow, int baseCol)
    {
        for (int r = 0; r < cell.rowspan; r++)
        {
            List<TablePrinterCell> layoutRow = getRow(layout, baseRow + r);
            for (int c = 0; c < cell.colspan; c++)
            {
                if (layoutRow.Count - 1 <= baseCol + c)
                {
                    for (int k = layoutRow.Count - 1; k < baseCol + c; k++)
                    {
                        layoutRow.Add(null);
                    }
                }
                layoutRow[baseCol + c] = cell;
            }
        }
    }

    public static int getOrInitOffset(List<int> offsets, int index)
    {
        if (offsets.Count <= index)
        {
            offsets.Add(index == 0 ? 0 : 1 + getOrInitOffset(offsets, index - 1));
        }
        return offsets[index];
    }

    public static void updateOffset(List<int> offsets, int @base, int span, int value)
    {
        offsets[@base + span] = Math.Max(
            getOrInitOffset(offsets, @base + span),
            getOrInitOffset(offsets, @base) + value
        );
    }

    public static string tableToString(List<List<TablePrinterCell>> tableRows, int rowSpacing, int colSpacing)
    {
        List<List<TablePrinterCell>> layout = new List<List<TablePrinterCell>>();
        int colNumber = 0;
        int rowNumber = tableRows.Count;
        List<int> rowOffsets = new List<int> { 0 };

        for (int j = 0; j < rowNumber; j++)
        {
            List<TablePrinterCell> layoutRow = getRow(layout, j);
            List<TablePrinterCell> cells = tableRows[j];
            int x = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                TablePrinterCell cell = cells[i];
                x = findFirstVacantIndex(layoutRow, x);
                putCellIntoLayout(cell, layout, j, x);
                x += cell.colspan;
                cell.lines = cell.text.Split('\n');
                int cellHeight = cell.lines.Length;
                updateOffset(rowOffsets, j, cell.rowspan, cellHeight + rowSpacing);
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
        
        transposeInPlace(layout, rowNumber > colNumber ? rowNumber : colNumber);
        List<string> outputLines = new List<string>();
        List<int> colOffsets = new List<int>() { 0 };

        for (int x = 0; x < colNumber; x++)
        {
            int y = 0;
            TablePrinterCell cell;
            int rowsInThisColumn = Math.Min(rowNumber, layout[x].Count);
            while (y < rowsInThisColumn)
            {
                cell = layout[x][y];
                if (cell != null)
                {
                    if (!cell.rendered)
                    {
                        int cellWidth = 0;
                        for (int j = 0; j < cell.lines.Length; j++)
                        {
                            string line = cell.lines[j];
                            int lineOffset = rowOffsets[y] + j;
                            if (outputLines.Count - 1 < lineOffset)
                            {
                                for (int k = outputLines.Count - 1; k < lineOffset; k++)
                                {
                                    outputLines.Add(null);
                                }
                            }

                            var colOffset = 0;
                            if (colOffsets.Count > x)
                            {
                                colOffset = getOrInitOffset(colOffsets, x);
                            }
                            
                            outputLines[lineOffset] = (outputLines[lineOffset] ?? "").PadRight(colOffset) + line;
                            
                            cellWidth = line.Length > cellWidth ? line.Length : cellWidth;
                        }
                        updateOffset(colOffsets, x, cell.colspan, cellWidth + colSpacing);
                        cell.rendered = true;
                    }
                    y += cell.rowspan;
                }
                else
                {
                    int lineOffset = rowOffsets[y];
                    
                    if (outputLines.Count - 1 < lineOffset)
                    {
                        for (int k = outputLines.Count - 1; k < lineOffset; k++)
                        {
                            outputLines.Add(null);
                        }
                    }
                    
                    outputLines[lineOffset] = outputLines[lineOffset] ?? "";
                    y++;
                }
            }
        }
        return string.Join("\n", outputLines);
    }
}