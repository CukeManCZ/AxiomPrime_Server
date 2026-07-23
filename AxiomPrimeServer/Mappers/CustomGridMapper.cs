using Utilities.DataStructures;

public static class CustomGridMapper
{
    public static CustomGridDto<T> ToDto<T>(CustomGrid<T> grid)
    {
        grid.GetSize(out int width, out int height);

        return new CustomGridDto<T>
        {
            Width = width,
            Height = height,
            Values = grid.ToList()
        };
    }


    public static CustomGrid<T> FromDto<T>(CustomGridDto<T> dto)
    {
        var grid = new CustomGrid<T>(
            dto.Width,
            dto.Height
        );

        int index = 0;

        for(int x = 0; x < dto.Width; x++)
        {
            for(int y = 0; y < dto.Height; y++)
            {
                grid.SetValue(
                    x,
                    y,
                    dto.Values[index++]
                );
            }
        }

        return grid;
    }
}