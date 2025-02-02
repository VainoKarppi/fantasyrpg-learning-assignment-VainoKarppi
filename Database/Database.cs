using Microsoft.EntityFrameworkCore;

public partial class Database {
    public static bool Initialize() {
        bool exists = File.Exists(Config.Instance.Database.Name);
        if (!exists) {
            using var db = new MyDbContext();
            db.Database.EnsureCreated();
        }

        ApplyMigrations();
        return true;
    }

    private static void ApplyMigrations() {
        using var db = new MyDbContext();
        db.Database.Migrate();
    }

    public static void ClearDatabase() {
        using var db = new MyDbContext();
        db.Database.EnsureDeleted();
    }
}