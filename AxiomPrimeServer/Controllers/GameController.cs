using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Utilities.AuthorizationTools;

[ApiController]
[Route("api/game")]
public class GameController : ControllerBase
{
    private readonly InventoryAPI m_inventoryAPI;
    private readonly ShipInventoryAPI m_shipInventoryAPI;

    public GameController(InventoryAPI inventoryAPI, ShipInventoryAPI shipInventoryAPI)
    {
        m_inventoryAPI = inventoryAPI;
        m_shipInventoryAPI = shipInventoryAPI;
    }

    [Authorize]
    [HttpGet("addItem")]
    public async Task<IActionResult> AddItem()
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        ItemGenerator itemGenerator = new ItemGenerator();
        itemGenerator.Initialize(new());

        Item item = itemGenerator.GenerateItem(1);
        if(await m_inventoryAPI.AddItem(profileId, item))
            return Ok(m_inventoryAPI.GetAsync(profileId).Result.Items);
        return NotFound();
    }

    [Authorize]
    [HttpGet("removeItem")]
    public async Task<IActionResult> RemoveItem()
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        Guid guid = Guid.Parse("d87b8702-31c9-43a2-b94b-5fb44ffc3e6a");
        if(await m_inventoryAPI.RemoveItem(profileId, guid))
            return Ok();
        return NotFound();
    }

    [HttpGet("equipItem")]
    public async Task<IActionResult> EquipItem()
    {
        var profileId = "a8ec2ea7-ef5f-402c-96c4-bfda58f40760";

        Inventory inventory = await m_inventoryAPI.GetAsync(profileId);
        Item? item = inventory.Items.First();
        ShipInventory shipInventory = await m_shipInventoryAPI.GetAsync(profileId);
        Ship? ship = shipInventory.Ships.First();
        if(item != null && ship != null)
        {
            if(!item.IsEquipped)
            {
                if(await m_shipInventoryAPI.PlaceItem(ship.Id, item, 0, 0))
                {
                    await m_inventoryAPI.EquipItem(profileId, item.Id);
                    return Ok();
                }
            }
        }
        return NotFound();
    }

    [HttpGet("unequipItem")]
    public async Task<IActionResult> UnEquipItem()
    {
        var profileId = "a8ec2ea7-ef5f-402c-96c4-bfda58f40760";

        Inventory inventory = await m_inventoryAPI.GetAsync(profileId);
        Item? item = inventory.Items.First();
        ShipInventory shipInventory = await m_shipInventoryAPI.GetAsync(profileId);
        Ship? ship = shipInventory.Ships.First();
        if(item != null && ship != null)
        {
            if (item.IsEquipped)
            {
                if(await m_shipInventoryAPI.RemoveItem(ship.Id, item.Id))
                {
                    await m_inventoryAPI.UnEquipItem(profileId, item.Id);
                    return Ok();
                }
            }
        }
        return NotFound();
    }
}