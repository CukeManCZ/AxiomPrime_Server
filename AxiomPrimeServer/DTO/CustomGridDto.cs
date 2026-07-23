using System.Collections.Generic;

public class CustomGridDto<T>
{
    public int Width { get; set; }
    public int Height { get; set; }

    public List<T> Values { get; set; } = new();
}