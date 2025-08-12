using System.Timers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SlovniFotbal.ViewModels;

public partial class GameWindowViewModel : ViewModelBase
{
    [ObservableProperty] private Controls _messages = new Controls();
    [ObservableProperty] private string _newWord;
    [ObservableProperty] private string _activePlayer = string.Empty;
    
    private int _playerNumber = 1;
    private readonly MainWindowViewModel _mainWindow;
    private readonly GameType _gameType;
    private readonly Timer _timer;
    
    public GameWindowViewModel(MainWindowViewModel mainWindow, GameType gameType)
    {
        this._mainWindow = mainWindow;
        this._gameType = gameType;
        
        ActivePlayer = $"Hraje: Hráč {_playerNumber}";
    }
    
    [RelayCommand]
    private void Close()
    {
        _mainWindow.BackToStartScreen();
    }

    [RelayCommand]
    private void Send()
    {
        AddMessage(NewWord);
        NewWord = string.Empty;
        
    }
    
    private void AddMessage(string message)
    {
        // add the message
        var control = new Border();
        var textBlock = new TextBlock { Text = message };
        control.Child = textBlock;
        Messages.Add(control);
        
        // change the active player
        _playerNumber = (_playerNumber++)%2+1;
        ActivePlayer = $"Hraje: Hráč {_playerNumber}";
    }
}

public enum GameType
{
    TwoPlayers,
    PlayerVsComputer
}