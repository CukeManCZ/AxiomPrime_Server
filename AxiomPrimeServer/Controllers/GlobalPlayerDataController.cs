using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("globalData")]
public class GlobalPlayerDataController : ControllerBase
{
    private readonly GlobalPlayerDataAPI m_globalPlayerDataAPI;

    public GlobalPlayerDataController(GlobalPlayerDataAPI globalPlayerDataAPI)
    {
        m_globalPlayerDataAPI = globalPlayerDataAPI;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        var profileId = User.FindFirst("profileId")?.Value;

        if (profileId == null)
            return Unauthorized();

        GlobalPlayerDataDTO result = await m_globalPlayerDataAPI.GetAsync(profileId);

        return Ok(result);
    }

    [Authorize]
    [HttpPut("addMoney/{money}")]
    public async Task<IActionResult> AddMoney(int money)
    {
        var profileId = User.FindFirst("profileId")?.Value;
        if(profileId == null)
            return Unauthorized();

        await m_globalPlayerDataAPI.AddMoney(profileId, money);

        return Ok();
    }
}