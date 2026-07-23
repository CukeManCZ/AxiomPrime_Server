using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("bc")]
public class BrainCloudController : ControllerBase
{
    private readonly BrainCloudService _bc;

    public BrainCloudController(BrainCloudService bc)
    {
        _bc = bc;
    }

    [HttpGet("bots")]
    public async Task<IActionResult> GetBots()
    {
        var result = await _bc.GetBots();
        return Ok(result);
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateBrainCloudUserDTO dto)
    {
        var result = await _bc.CreateUserEmailPassword(
            dto.Email,
            dto.Password,
            dto.UserName,
            dto.NotificationTemplateId
        );

        return Ok(result);
    }

    [HttpDelete("delete-user/{profileId}")]
    public async Task<IActionResult> DeleteUser(string profileId)
    {
        var result = await _bc.DeleteUser(profileId);

        return Ok(result);
    }

    [HttpGet("user/{profileId}")]
    public async Task<IActionResult> GetUser(string profileId)
    {
        var user = await _bc.GetUserInfo(profileId);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    
}