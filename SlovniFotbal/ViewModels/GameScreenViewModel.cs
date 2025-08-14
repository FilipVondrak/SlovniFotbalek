using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SlovniFotbal.Models;
using SlovniFotbal.Views;

namespace SlovniFotbal.ViewModels;

public partial class GameScreenViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty] private ObservableCollection<string> _messages = new();
    [ObservableProperty] private string _newWord = string.Empty;
    [ObservableProperty] private string _activePlayer = string.Empty;
    [ObservableProperty] private char? _lastChar = null;
    [ObservableProperty] private bool _inputAllowed = true;
    
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
    private readonly GameScreen _view;

    public int Seconds
    {
        get => _seconds;
        set
        {
            _seconds = value;
            OnPropertyChanged();
            
            if (_seconds <= 0)
            {
                _timer.Stop();
                if (_gameType == GameType.TwoPlayers)
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        ShowWinScreen($"Hráč {_playerNumber}");
                    });
                }
                else if (_gameType == GameType.PlayerVsComputer)
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        ShowWinScreen("Počítač");
                    });
                }
            }
        }
    }
    
    public GameScreenViewModel(MainWindowViewModel mainWindow, Game game, GameScreen view)
    {
        this._mainWindow = mainWindow;
        this._view = view;
        
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
    private async void Save()
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(_view);

        // Start async operation to open the dialog.
        IStorageFile file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Text File"
        });

        if(file is null)
            return;
        
        var game = new Game(messages: Messages.ToList(), activePlayerNumber: _playerNumber, seconds: Seconds, mode: _gameType);
        await GameSerializer.SerializeGame(game, file);
    }

    [RelayCommand]
    private async void Send(string input = "")
    {
        _timer.Stop();
        
        // create a new message and display it
        // if the inputted new word is invalid, then the player continues guessing
        // if the method is called by AI (input isnt null) then instead of using player input create word from AI input
        if (await AddMessage(!string.IsNullOrWhiteSpace(input)  ? input : NewWord) == false)
        {
            _timer.Start();
            return;
        }
        NewWord = string.Empty;

        // change the active player
        _playerNumber = _playerNumber%2+1;
        
        if (_gameType == GameType.TwoPlayers || _playerNumber == 1)
        {
            ActivePlayer = $"Hraje: Hráč {_playerNumber}";
            InputAllowed = true;
        
            // restart the timer
            Seconds = 30;
            _timer.Start();
        }
        else if (_gameType == GameType.PlayerVsComputer)
        {
            ActivePlayer = "Hraje: Počítač";
            InputAllowed = false;
            Seconds = 30;
            _timer.Start();
            AiSendMessage();
        }
    }

    private async Task AiSendMessage()
    {
        var random = new Random();
        await Task.Delay(random.Next(1000, 5000));
        using (var db = new WordContext())
        {
            // queries the database for the word
            string? word = await db.Words
                .OrderBy(w => EF.Functions.Random())
                // limits the AIs word list to only 5k random words
                .Take(5000)
                // selects words that start with the correct letter
                .Where(w => w.Word.StartsWith(LastChar.ToString()!.ToLower()))
                // checks if the word hasnt been used yet
                .Where(w => !Messages.Contains(w.Word))
                // randomizes the words
                .OrderBy(w => EF.Functions.Random()) // SQLite random order
                // only keeps the word string
                .Select(w => w.Word)
                // takes the first word
                .FirstOrDefaultAsync();

            // if the ai coulnt find any valid word then the player won
            if (word == null)
            {
                ShowWinScreen("Hráč 1");
                return;
            }
            
            Send(word);
        }
    }

    private async Task ShowWinScreen(string player)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard("Konec hry", $"Vyhrál {player}!",
                ButtonEnum.Ok);

        await box.ShowWindowAsync();
        Close();
    }
    
    private async Task<bool> AddMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            ShowError("Špatné slovo!");
            return false;
        }
        
        if (message.Contains(" "))
        {
            ShowError("Zadejte pouze jedno slovo!");
            return false;
        }

        // check if the word starts with the correct letter
        if (message.First() != LastChar && LastChar != null)
        {
            ShowError("Slovo nezačíná správným písmenkem!");
            return false;
        }

        // make sure the word hasnt been used yet
        if (Messages.Contains(message))
        {
            ShowError("Slovo už bylo použito!");
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