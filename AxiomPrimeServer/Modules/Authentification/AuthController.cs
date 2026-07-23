using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Utilities.DataStructures;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly BrainCloudService m_brainCloud;
    private readonly IConfiguration m_config;
    private readonly PlayerRepository m_playerService;
    private readonly InventoryAPI m_inventoryAPI;
    private readonly ShipInventoryAPI m_shipInventoryAPI;

    public AuthController(
        BrainCloudService brainCloud,
        IConfiguration config,
        PlayerRepository playerService,
        InventoryAPI inventoryAPI,
        ShipInventoryAPI shipInventoryAPI)
    {
        m_brainCloud = brainCloud;
        m_config = config;
        m_playerService = playerService;
        m_inventoryAPI = inventoryAPI;
        m_shipInventoryAPI = shipInventoryAPI;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Auth_Request_DTO req)
    {
        // 1. verify BrainCloud user exists
        string decryptedID = CryptoUtils.Decrypt(req.ProfileId);
        var brainUser = await m_brainCloud.GetUserInfo(decryptedID);

        if (brainUser == null)
            return Unauthorized("Invalid BrainCloud user");

        // 2. get or create player
        var player = await m_playerService.GetPlayer(decryptedID);

        if (player == null)
        {
            //Create player
            player = await m_playerService.CreatePlayer(
                brainUser.PlayerName,
                brainUser.ProfileId,
                brainUser.EmailAddress
            );
            //Add default ships
            CustomGrid<string> ship = new CustomGrid<string>(3,3, "Empty");
            ShipGrid shipGrid = ShipGrid.FromCustomGrid(ship);
            await m_shipInventoryAPI.CreateShip(player.Id, shipGrid);
            //Add default items
            ItemGenerator itemGenerator = new ItemGenerator();
            itemGenerator.Initialize(new());

            for(int i = 0; i < 5; i++){
                await m_inventoryAPI.AddItem(player.Id, itemGenerator.GenerateItem(1));
            }
        }

        // 3. issue JWT
        var token = CreateToken(player.Id);

        return Ok(new
        {
            token,
            profileId = player.Id
        });
    }

    [HttpPost("check-register")]
    public async Task<IActionResult> CheckRegister([FromBody] RegisterCheck_Request_DTO request)
    {
        var result = await m_playerService.CheckRegisterAsync(
            request.Email,
            request.Username);

        return Ok(result);
    }

    private string CreateToken(string profileId)
    {
        var jwtKey = m_config["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key missing in appsettings.json");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("profileId", profileId)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class Auth_Request_DTO
{
    public required string ProfileId { get; set; }
}

public class RegisterCheck_Request_DTO
{
    public required string Email { get; set; }
    public required string Username { get; set; }
}

public class RegisterCheck_Response_DTO
{
    public bool EmailExists { get; set; }
    public bool UsernameExists { get; set; }
}