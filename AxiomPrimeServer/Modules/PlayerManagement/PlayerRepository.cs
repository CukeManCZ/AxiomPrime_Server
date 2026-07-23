using Microsoft.EntityFrameworkCore;

public class PlayerRepository
{
    private readonly GameDbContext _db;

    public PlayerRepository(GameDbContext db)
    {
        _db = db;
    }

    public async Task<PlayerDto> CreatePlayer(string username, string id, string email)
    {
        var player = new Player
        {
            Username = username,
            Id = id,
            Email = email
        };

        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        return new PlayerDto
        {
            Id = player.Id,
            Username = player.Username,
            Email = player.Email
        };
    }

    public async Task<List<PlayerDto>> GetAllPlayers()
    {
        return await _db.Players
            .Select(p => new PlayerDto
            {
                Id = p.Id,
                Username = p.Username,
                Email = p.Email
            })
            .ToListAsync();
    }

    public async Task<PlayerDto?> GetPlayer(string id)
    {
        return await _db.Players
            .Where(p => p.Id == id)
            .Select(p => new PlayerDto
            {
                Id = p.Id,
                Username = p.Username,
                Email = p.Email
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeletePlayer(int id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null)
            return false;

        _db.Players.Remove(player);
        await _db.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Check if user is registered returns RegisterCheck_Response_DTO
    /// </summary>
    /// <param name="email"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public async Task<RegisterCheck_Response_DTO> CheckRegisterAsync(string email, string username)
    {
        bool emailExists = await _db.Players
            .AnyAsync(x => x.Email == email);

        bool usernameExists = await _db.Players
            .AnyAsync(x => x.Username == username);

        return new RegisterCheck_Response_DTO
        {
            EmailExists = emailExists,
            UsernameExists = usernameExists
        };
    }
}