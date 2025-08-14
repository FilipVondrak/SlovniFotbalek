using System.Collections.Generic;
using System.Text;
using SlovniFotbal.ViewModels;

namespace SlovniFotbal;

public class Game
{
    public Game(GameType mode, List<string> messages = null, int activePlayerNumber = 1, int seconds = 30, char? lastChar = null)
    {
        this.Messages = messages;
        this.ActivePlayerNumber = activePlayerNumber;
        this.Seconds = seconds;
        this.Mode = mode;
    }
        
    public List<string> Messages { get; set; }
    public int ActivePlayerNumber { get; set; }
    public int Seconds { get; set; }
    
    public char? LastChar { get; set; }
    public GameType Mode { get; set; }
}