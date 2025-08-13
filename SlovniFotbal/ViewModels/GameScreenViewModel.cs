using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SlovniFotbal.ViewModels;

public partial class GameScreenViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty] private ObservableCollection<string> _messages = new();
    [ObservableProperty] private string _newWord;
    [ObservableProperty] private string _activePlayer = string.Empty;
    
    private int _seconds = 30;

    public int Seconds
    {
        get => _seconds;
        set
        {
            _seconds = value;
            OnPropertyChanged();
            
            if (_seconds <= 0)
            {
                Environment.Exit(0);
            }
        }
    }
    
    private int _playerNumber = 1;
    private readonly MainWindowViewModel _mainWindow;
    private readonly GameType _gameType;
    private readonly Timer _timer;
    private readonly ElapsedEventHandler _decreaseSeconds;
    
    public GameScreenViewModel(MainWindowViewModel mainWindow, Game game)
    {
        this._mainWindow = mainWindow;
        
        // load the game
        this.Seconds = game.Seconds;
        this._playerNumber = game.ActivePlayerNumber;
        this._gameType = game.Mode;
        
        // loads the messages
        if (game.Messages != null && game.Messages.Count > 0)
        {
            foreach (string message in game.Messages)
            {
                AddMessage(message);
            }
        }
        
        // setup timer
        this._timer = new Timer(1000);
        this._decreaseSeconds = (s, e) => Seconds--;
        this._timer.Elapsed += _decreaseSeconds;
        
        // set the active player
        ActivePlayer = $"Hraje: Hráč {_playerNumber}";
        
        // start the timer
        _timer.Start();
    }
    
    public void Dispose()
    {
        // Stop the timer and unsubscribe from the event to break the reference cycle.
        _timer.Stop();
        _timer.Elapsed -= _decreaseSeconds;
        _timer.Dispose();
        Messages.Clear(); // Also clear the collection of controls.
    }
    
    [RelayCommand]
    private void Close()
    {
        Dispose();
        _mainWindow.BackToStartScreen();
    }

    [RelayCommand]
    private void Save()
    {
        var game = new Game(messages: Messages.ToList(), activePlayerNumber: _playerNumber, seconds: Seconds, mode: _gameType);
        GameSerializer.SerializeGame(game, "game");
    }

    [RelayCommand]
    private void Send()
    {
        _timer.Stop();
        
        // create a new message and display it
        AddMessage(NewWord);
        NewWord = string.Empty;
        
        // change the active player
        _playerNumber = _playerNumber%2+1;
        ActivePlayer = $"Hraje: Hráč {_playerNumber}";
        
        // restart the timer
        Seconds = 30;
        _timer.Start();
    }
    
    private void AddMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            Messages.Add(message);
    }
}

public enum GameType
{
    TwoPlayers,
    PlayerVsComputer
}