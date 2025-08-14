using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SlovniFotbal.ViewModels;
using SlovniFotbal.Views;

namespace SlovniFotbal;

public static class GameSerializer
{
    public static async Task SerializeGame(Game game, IStorageFile file)
    {
        string jsonString = JsonSerializer.Serialize(game);
        
        //File.WriteAllText($"{name}.json", jsonString);
        await using var stream = await file.OpenWriteAsync();
        using var streamWriter = new StreamWriter(stream);
        // Write some content to the file.
        await streamWriter.WriteLineAsync(jsonString);
    }

    public static async Task<Game> DeserializeGame(UserControl screen)
    {
        var topLevel = TopLevel.GetTopLevel(screen);
        
        if  (topLevel == null)
            throw new Exception("No top level found");
        
        // Start async operation to open the dialog.
        var file = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        // check if at least one file is selected, otherwise create a new one
        if (file.Count < 1)
            return new Game(mode: GameType.TwoPlayers); 
                
        await using var stream = await file[0].OpenReadAsync();
        using var streamReader = new StreamReader(stream);
        // Reads all the content of file as a text.
        var fileContent = await streamReader.ReadToEndAsync();
        
        // loads game save from json or creates a default game
        return JsonSerializer.Deserialize<Game>(fileContent) ?? throw new Exception("No file found");
    }
}