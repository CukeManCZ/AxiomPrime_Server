using Microsoft.EntityFrameworkCore;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<Currencies> Currencies {get; set;}
    public DbSet<Experience> Experiences {get; set;}
    public DbSet<Energy> Energies {get; set;}

    public DbSet<Inventory> Inventories {get; set;}
    public DbSet<Item_Database> Items { get; set; }

    public DbSet<ShipInventory> ShipInventories {get; set;}
    public DbSet<Ship_Database> Ships {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Currencies>()
            .HasKey(x => x.PlayerID); 

        modelBuilder.Entity<Experience>()
            .HasKey(x => x.PlayerID);

        modelBuilder.Entity<Energy>()
            .HasKey(x => x.PlayerID);

        modelBuilder.Entity<Player>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder.Entity<Inventory>(inv =>
        {
            inv.HasKey(x => x.PlayerId);

            inv.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey("InventoryPlayerId")
                .HasPrincipalKey(x => x.PlayerId);
        });

        modelBuilder.Entity<Item_Database>(item =>
        {
            item.HasKey(i => i.Id);

            item.Property(i => i.Id)
                .ValueGeneratedNever();

            item.Property(i => i.ItemDataJson)
                .HasColumnName("ItemData")
                .HasColumnType("jsonb")
                .IsRequired();

            item.ToTable("Item");
        });


        //Ship Inventory
        modelBuilder.Entity<ShipInventory>()
            .HasKey(x => x.PlayerId);

        modelBuilder.Entity<ShipInventory>()
            .HasMany(x => x.Ships)
            .WithOne(x => x.ShipInventory)
            .HasForeignKey(x => x.ShipInventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ship_Database>()
            .HasMany(x => x.Items)
            .WithOne(x => x.Ship)
            .HasForeignKey(x => x.ShipId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShipItem>(shipItem =>
        {
            shipItem.Property(x => x.Id)
                .ValueGeneratedNever();

            shipItem.Property(x => x.ItemId)
                .ValueGeneratedNever();

            shipItem.HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Ship_Database>(ship =>
        {
            ship.HasKey(x => x.Id);

            ship.Property(x => x.Id)
                .ValueGeneratedNever();

            ship.Property(x => x.ShipDataJson)
                .HasColumnName("ShipData")
                .HasColumnType("jsonb")
                .IsRequired();
        });
    }
}