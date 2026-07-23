using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Utilities.AuthorizationTools;

[ApiController]
[Route("inventory")]
public class InventoryController : ControllerBase
{
    private readonly InventoryAPI m_inventoryAPI;
    private readonly ShipInventoryAPI m_shipInventoryAPI;

    public InventoryController(InventoryAPI inventoryAPI, ShipInventoryAPI shipInventoryAPI)
    {
        m_inventoryAPI = inventoryAPI;
        m_shipInventoryAPI = shipInventoryAPI;
    }

    #region Data gathering

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetInventory()
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        Inventory inventory = await m_inventoryAPI.GetAsync(profileId);
        InventoryDto inventoryDto = InventoryMapper.ToDto(inventory);
        return Ok(inventoryDto);
    }

    [Authorize]
    [HttpGet("ship")]
    public async Task<IActionResult> GetShipInventory()
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        var shipInventory = await m_shipInventoryAPI.GetAsync(profileId);
        var shipInventoryDto = ShipMapper.ToDto(shipInventory);
        return Ok(shipInventoryDto);
    }

    [Authorize]
    [HttpGet("ship/{shipId:guid}")]
    public async Task<IActionResult> GetShip(Guid shipId)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        var ship = await m_shipInventoryAPI.GetShipAsync(shipId);
        if (ship.ShipInventoryId != profileId)
            return Forbid();

        return Ok(ShipMapper.ToDto(ship));
    }

    #endregion

    #region Item Manipulation
    //Destroy item
    [Authorize]
    [HttpDelete("{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        var result = await m_inventoryAPI.RemoveItem(profileId, itemId);
        if (!result)
            return BadRequest("Item not removed");

        return Ok();
    }

    [Authorize]
    [HttpPost("ship/{shipId:guid}/equip/{itemId:guid}")]
    public async Task<IActionResult> EquipItem(Guid shipId, Guid itemId, [FromQuery] int x, [FromQuery] int y)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        var inventory = await m_inventoryAPI.GetAsync(profileId);
        var item = inventory.Items.FirstOrDefault(x => x.Id == itemId);
        var ship = await m_shipInventoryAPI.GetShipAsync(shipId);
        if (ship.ShipInventoryId != profileId)
            return Forbid();
        if (item == null)
            return Forbid();

        //Get if item is equiped in ship
        if (item.IsEquipped)
        {
            // try to move item in ship
            var previousItem = ship.Items.FirstOrDefault(x => x.ItemId == item.Id);
            if(previousItem != null)
            {
                //Try to move item in ship
                bool itemReset = false;

                if(!await m_shipInventoryAPI.RemoveItem(shipId, itemId)) itemReset = true;
                if(!await m_inventoryAPI.UnEquipItem(profileId, itemId)) itemReset = true;
                if(!await m_shipInventoryAPI.PlaceItem(shipId, item, x, y)) itemReset = true;
                if(!await m_inventoryAPI.EquipItem(profileId, itemId)) itemReset = true;

                if (itemReset)
                {
                    await m_shipInventoryAPI.RemoveItem(shipId, itemId);
                    await m_inventoryAPI.UnEquipItem(profileId, itemId);
                    await m_shipInventoryAPI.PlaceItem(shipId, item, previousItem.X, previousItem.Y);
                    await m_inventoryAPI.EquipItem(profileId, itemId);
                }
                return Ok();
            }
        }

        //Try to equip normally
        var shipItem = await m_shipInventoryAPI.GetItemAt(ship, x, y);
        if(shipItem == null)
        {
            //Place item normally
            if(await m_shipInventoryAPI.PlaceItem(shipId, item, x, y))
                if(await m_inventoryAPI.EquipItem(profileId, itemId))
                    return Ok();
        }
        else
        {
            //Try to swap items if same size
            if(shipItem.Item.Size.ToCustomGrid() == item.Size.ToCustomGrid())
            {
                //Remove the old item from ship
                await m_shipInventoryAPI.RemoveItem(shipId, shipItem.Id);
                await m_inventoryAPI.UnEquipItem(profileId, shipItem.Item.Id);
                //Place new in ship
                await m_shipInventoryAPI.PlaceItem(shipId, item, x, y);
                await m_inventoryAPI.EquipItem(profileId, item.Id);
                return Ok();
            }
        }
            
        return BadRequest("Item not equipped");
    }

    [Authorize]
    [HttpPost("{itemId:guid}/unequip")]
    public async Task<IActionResult> UnEquipItem(Guid itemId)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();


        Inventory inventory = await m_inventoryAPI.GetAsync(profileId);
        Item? item = inventory.Items.FirstOrDefault(x => x.Id == itemId);
        if(item == null)
            return BadRequest("Item does not exist");

        ShipInventory shipInventory = await m_shipInventoryAPI.GetAsync(profileId);
        Ship? ship = null;
        foreach(Ship s in shipInventory.Ships)
        {
            foreach(ShipItem shipItem in s.Items)
                if(shipItem.Id == item.Id)
                {
                    ship = s;
                    break;
                }
        }

        if(ship == null)
            return BadRequest("Item does not exist in ship");
        
        var result = await m_shipInventoryAPI.RemoveItem(ship.Id, item.Id) && await m_inventoryAPI.UnEquipItem(profileId, itemId);
        
        return result ? Ok() : BadRequest("Item not dequipped");
    }

    [Authorize]
    [HttpPost("ship/{shipId:guid}/dequip")]
    public async Task<IActionResult> DequipShipItems(Guid shipId)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        var ship = await m_shipInventoryAPI.GetShipAsync(shipId);
        if (ship.ShipInventoryId != profileId)
            return Forbid();

        foreach (var shipItem in ship.Items.ToList())
        {
            var removed = await m_shipInventoryAPI.RemoveItem(shipId, shipItem.Id);
            if (!removed)
                continue;

            await m_inventoryAPI.AddItem(profileId, shipItem.Item);
        }

        return Ok();
    }

    #endregion

    #region ShipSlot
    //Slots management new slots unlock
    [Authorize]
    [HttpPost("ship/{shipId:guid}/unlock")]
    public async Task<IActionResult> TryToUnlockSlot(Guid shipId, [FromQuery] int x, [FromQuery] int y)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        var ship = await m_shipInventoryAPI.GetShipAsync(shipId);
        if (ship.ShipInventoryId != profileId)
            return Forbid();

        var result = await m_shipInventoryAPI.TryUnlockSlot(shipId, x, y);
        return result ? Ok() : BadRequest("Slot not unlocked");
    }
    #endregion

    private static bool SameSize(Item left, Item right)
        => left?.Size?.Width == right?.Size?.Width && left?.Size?.Height == right?.Size?.Height;

    //Stat computing
    //.....
}