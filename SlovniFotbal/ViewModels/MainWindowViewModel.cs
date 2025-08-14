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
        _startScreen = new();
        _currentContent = _startScreen;
        _startScreen.DataContext = new StartScreenViewModel(this, _startScreen);
    }

    public void StartGame(GameScreen gameScreen) => CurrentContent = gameScreen;
    public void BackToStartScreen() => CurrentContent = _startScreen;

    private readonly StartScreen _startScreen;
    
    [ObservableProperty] private UserControl _currentContent;
}