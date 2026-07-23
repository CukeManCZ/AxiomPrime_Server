using System.Collections.Generic;

public class InventoryDto
{
    public int NumOfItems { get; set; }

    public List<ItemDto> Items { get; set; } = new();
}