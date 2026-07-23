using Utilities.DataStructures;

public class ShipGrid
{
    public int Width { get; set; }
    public int Height { get; set; }

    // flattened grid (row-major)
    public List<string> Cells { get; set; } = new();

    // helper index:
    public string Get(int x, int y)
        => Cells[y * Width + x];

    public void Set(int x, int y, string value)
        => Cells[y * Width + x] = value;


    #region CustomGrid
    public CustomGrid<string> ToCustomGrid()
    {
        var grid = new CustomGrid<string>(Width, Height);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid.SetValue(x, y, Get(x, y));
            }
        }

        return grid;
    }

    public void SetFromCustomGrid(CustomGrid<string> grid)
    {
        grid.GetSize(out int width, out int height);

        Width = width;
        Height = height;

        Cells.Clear();
        Cells.Capacity = width * height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Cells.Add(grid.GetValue(x, y));
            }
        }
    }

    public static ShipGrid FromCustomGrid(CustomGrid<string> grid)
    {
        var result = new ShipGrid();
        result.SetFromCustomGrid(grid);
        return result;
    }
    #endregion
}