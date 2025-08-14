using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SlovniFotbal.ViewModels;

namespace SlovniFotbal.Views;

public partial class GameScreen : UserControl
{
    public GameScreen()
    {
        InitializeComponent();
    }
    
    public GameScreen(MainWindowViewModel main, Game game)
    {
        InitializeComponent();
        this.DataContext = new GameScreenViewModel(main, game, this);
        var vm = (GameScreenViewModel) DataContext;
        vm.Messages.CollectionChanged += MessageAdded;
    }

    private void MessageAdded(object? sender, EventArgs e) => ScrollViewer.ScrollToEnd();
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (DataContext is GameScreenViewModel vm)
        {
            vm.Messages.CollectionChanged -= MessageAdded;
            vm.Dispose();
            DataContext = null;
        }
    }
}