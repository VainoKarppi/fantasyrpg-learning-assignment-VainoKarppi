using System.Text.Json;
using System.Threading.Tasks;

public partial class Database {
    public static event Action<NpcCharacter>? OnNpcRestored;
    public static event Action<NpcCharacter>? OnNpcSaved;

    public static async Task SaveAllNpcs() {
        foreach (World world in GameInstance.Worlds) {
            foreach (NpcCharacter npc in world.NPCs){
                await SaveNpcAsync(npc);
            }
        }
    }
    public static async Task SaveNpcAsync(NpcCharacter npc) {
        using var db = new MyDbContext();
        var npcEntity = db.Npcs.FirstOrDefault(n => n.ID == npc.ID);
        if (npcEntity == null) {
            npcEntity = new NpcEntity();
            db.Npcs.Add(npcEntity);
        }

        npcEntity.ID = npc.ID;
        npcEntity.Health = npc.Health;
        npcEntity.Mana = npc.Mana;
        npcEntity.Name = npc.Name!;
        
        npcEntity.X = npc.X;
        npcEntity.Y = npc.Y;
        npcEntity.Width = npc.Width;
        npcEntity.Height = npc.Height;

        npcEntity.CurrentWeapon = npc.CurrentWeapon?.Name;
        npcEntity.CurrentArmor = npc.CurrentArmor?.Name;
        npcEntity.CurrentWorld = npc.CurrentWorld.Name;

        await db.SaveChangesAsync();

        OnNpcSaved?.InvokeFireAndForget(npc);
    }

    public static async Task<bool> RemoveNPC(int npcID) {
        using var db = new MyDbContext();

        var npcEntity = db.Npcs.FirstOrDefault(n => n.ID == npcID);
        if (npcEntity == null) return false;

        db.Npcs.Remove(npcEntity);
        await db.SaveChangesAsync();

        return true;
    }

    public static List<NpcCharacter> RestoreNpcs() {
        using var db = new MyDbContext();

        List<NpcCharacter> npcs = [];
        foreach (NpcEntity npcEntity in db.Npcs) {
            if (npcEntity == null) continue;

            World world = GameInstance.GetWorld(npcEntity.CurrentWorld);
            if (world == null) continue;

            var npc = NpcCharacter.CreateNPC(npcEntity.Name, world, (npcEntity.X, npcEntity.Y), npcEntity.Health, false);
            npc.ID = npcEntity.ID;
            npc.Mana = npcEntity.Mana;

            npc.CurrentWeapon = ItemWeapon.GetWeaponByName(npcEntity.CurrentWeapon);
            npc.CurrentArmor = ItemArmor.GetArmorByName(npcEntity.CurrentArmor);

            npcs.Add(npc);

            OnNpcRestored?.InvokeFireAndForget(npc);
        }

        return npcs;
    }
}