using CommunityToolkit.Mvvm.Input;
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
    private void LoadGame()
    {
        var gameSettings = GameSerializer.DeserializeGame();
        var game = new GameScreen(_mainWindow, gameSettings);
        _mainWindow.StartGame(game);
    }
}