using System;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SlovniFotbal.Views;

namespace SlovniFotbal.ViewModels;

public partial class StartScreenViewModel : ViewModelBase
{
    private MainWindowViewModel _mainWindow;
    
    public StartScreenViewModel(MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;
    }
    
    [RelayCommand]
    private void StartTwoPlayers()
    {
        var gameSettings = new Game(mode: GameType.TwoPlayers);
        var game = new GameScreen(_mainWindow, gameSettings);
        _mainWindow.StartGame(game);
    }
    
    [RelayCommand]
    private async void LoadGame()
    {
        Game? gameSettings = null;

        try
        {
            gameSettings = GameSerializer.DeserializeGame();
        }
        catch (Exception e)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Varování", "Žádná uložená hra nebyla nalezena!",
                    ButtonEnum.Ok);

            await box.ShowWindowAsync();
        }

        if (gameSettings == null)
            return;
        
        var game = new GameScreen(_mainWindow, gameSettings);
        _mainWindow.StartGame(game);
    }
}