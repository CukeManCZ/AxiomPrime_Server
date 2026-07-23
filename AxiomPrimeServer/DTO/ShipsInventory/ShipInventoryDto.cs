using System.Collections.Generic;

public class ShipInventoryDto
{
    public int NumOfShips {get; set;}
    public List<ShipDto> Ships {get; set;} = new();
}