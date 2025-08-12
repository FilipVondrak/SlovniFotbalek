using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SlovniFotbal.Views;

namespace SlovniFotbal.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        _currentContent = _startScreen;
        _startScreen.DataContext = new StartScreenViewModel(this);
    }

    public void StartGame(GameScreen gameScreen) => CurrentContent = _gameScreen = gameScreen;
    public void BackToStartScreen()
    {
        CurrentContent = _startScreen;
        this._gameScreen = null;
    }

    private GameScreen _gameScreen;
    private readonly StartScreen _startScreen = new();
    
    [ObservableProperty] private UserControl _currentContent;
}