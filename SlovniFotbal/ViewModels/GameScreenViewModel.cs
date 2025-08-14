using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace SlovniFotbal.ViewModels;

public partial class GameScreenViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty] private ObservableCollection<string> _messages = new();
    [ObservableProperty] private string _newWord = string.Empty;
    [ObservableProperty] private string _activePlayer = string.Empty;
    [ObservableProperty] private char? _lastChar = null;
    
    [ObservableProperty] private string _errorText = string.Empty;
    [ObservableProperty] private bool _errorVisible = false;
    
    private int _playerNumber = 1;
    private readonly MainWindowViewModel _mainWindow;
    private readonly GameType _gameType;
    private readonly Timer _timer;
    private readonly ElapsedEventHandler _decreaseSeconds;
    private readonly Timer _errorTimer;
    private readonly ElapsedEventHandler _displayErrorMessage;
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
        
        // setup error timer
        this._errorTimer = new Timer(4000);
        this._displayErrorMessage = (s, e) =>
        {
            ErrorText = string.Empty;
            ErrorVisible = false;
            _errorTimer.Stop();
            _errorTimer.Elapsed -= _displayErrorMessage;
        };
        
        // set the active player
        ActivePlayer = $"Hraje: Hráč {_playerNumber}";
        
        // start the timer
        _timer.Start();
    }

    private void ShowError(string text)
    {
        ErrorText = text;
        ErrorVisible = true;
        _errorTimer.Elapsed += _displayErrorMessage;
        _errorTimer.Start();
    }
    
    public void Dispose()
    {
        _timer.Stop();
        _timer.Elapsed -= _decreaseSeconds;
        _timer.Dispose();
        
        _errorTimer.Stop();
        _errorTimer.Elapsed -= _displayErrorMessage;
        _errorTimer.Dispose();
        
        Messages.Clear();
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
    private async void Send()
    {
        _timer.Stop();
        
        // create a new message and display it
        // if the inputted new word is invalid, then the player continues
        if (await AddMessage(NewWord) == false)
        {
            _timer.Start();
            return;
        }
        NewWord = string.Empty;
        
        // change the active player
        _playerNumber = _playerNumber%2+1;
        ActivePlayer = $"Hraje: Hráč {_playerNumber}";
        
        // restart the timer
        Seconds = 30;
        _timer.Start();
    }
    
    private async Task<bool> AddMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            ShowError("Špatné slovo!");
            return false;
        }

        if (message.First() != LastChar && LastChar != null)
        {
            ShowError("Slovo nezačíná správným písmenkem!");
            return false;
        }

        using (var db = new WordContext())
        {
            var word = message.ToLower().Trim();
            bool exists = await db.Words.AnyAsync(w => w.Word == word);
            if (exists)
            {
                LastChar = message.Last();
                Messages.Add(message);
                
                // remove error message
                if (ErrorVisible)
                {
                    ErrorVisible = false;
                    ErrorText = string.Empty;
                    _errorTimer.Stop();
                    _errorTimer.Elapsed -= _displayErrorMessage;
                }
                
                return true;           
            }
            else
            {
                ShowError("Toto slovo neexistuje!");
                return false;           
            }
        }
    }
}

public enum GameType
{
    TwoPlayers,
    PlayerVsComputer
}