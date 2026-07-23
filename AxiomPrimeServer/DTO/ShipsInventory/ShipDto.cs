using Utilities.DataStructures;
using System.Collections.Generic;

public class ShipDto
{
    public Guid Id {get; set;}
    public bool IsLocked {get; set;}
    public int XOrigin {get; set;}
    public int YOrigin {get; set;}

    public CustomGridDto<string> Grid {get; set;} = default!;
    public List<ShipItemDto> Items {get; set;} = new();
}