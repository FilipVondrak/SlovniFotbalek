using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Controls;
using SlovniFotbal.ViewModels;

namespace SlovniFotbal;

public static class GameSerializer
{
    public static void SerializeGame(Game game, string name = "game")
    {
        string jsonString = JsonSerializer.Serialize(game);
        
        File.WriteAllText($"{name}.json", jsonString);
    }

    public static Game DeserializeGame(string name = "game")
    {
        // loads game save from json or creates a default game
        return JsonSerializer.Deserialize<Game>(File.ReadAllText($"{name}.json")) ?? 
               new Game(messages: new List<string>(), activePlayerNumber: 1, seconds: 30, mode: GameType.TwoPlayers); 
    }
}