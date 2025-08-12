using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SlovniFotbal.ViewModels;

namespace SlovniFotbal.Views;

public partial class GameScreen : UserControl
{
    public GameScreen()
    {
        
    }
    
    public GameScreen(MainWindowViewModel main, GameType gameType)
    {
        this.DataContext = new GameWindowViewModel(main, gameType);
        InitializeComponent();
    }

}