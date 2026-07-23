using Utilities.DataStructures;

public class ItemGridData
{
    public int Width { get; set; }
    public int Height { get; set; }

    // flattened grid
    public List<bool> Values { get; set; } = new();

    public bool Get(int x, int y)
        => Values[y * Width + x];

    public void Set(int x, int y, bool value)
        => Values[y * Width + x] = value;

    #region CustomGrid
    public CustomGrid<bool> ToCustomGrid()
    {
        var grid = new CustomGrid<bool>(Width, Height);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid.SetValue(x, y, Get(x, y));
            }
        }

        return grid;
    }
    
    public void SetFromCustomGrid(CustomGrid<bool> grid)
    {
        grid.GetSize(out int width, out int height);

        Width = width;
        Height = height;

        Values.Clear();
        Values.Capacity = width * height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Values.Add(grid.GetValue(x, y));
            }
        }
    }

    public static ItemGridData FromCustomGrid(CustomGrid<bool> grid)
    {
        var result = new ItemGridData();
        result.SetFromCustomGrid(grid);
        return result;
    }
    #endregion
}