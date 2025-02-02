using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Config {
    public List<WorldConfig> Worlds { get; set; }
    public DatabaseConfig Database { get; set; }
    public MultiplayerConfig Multiplayer { get; set; }

    public static Config Instance { get; private set; }

    public static void LoadConfig() {
        string configPath = "Config.json";
        if (!File.Exists(configPath)) throw new FileNotFoundException($"Configuration file not found: {configPath}");

        string json = File.ReadAllText(configPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Instance = JsonSerializer.Deserialize<Config>(json, options)!;
    }
    public class MultiplayerConfig {
        public int ServerPort { get; set; }
    }
    public class DatabaseConfig {
        public string Name { get; set; }
    }
    public class WorldConfig {
        public string Name { get; set; }
        public List<string> Enemies { get; set; }
        public List<BuildingConfig> Buildings { get; set; }
    }

    public class BuildingConfig {
        public string Name { get; set; }
        public string Type { get; set; }
        public Position Position { get; set; }
        public Size Size { get; set; }
    }

    public class Position {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Size {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}