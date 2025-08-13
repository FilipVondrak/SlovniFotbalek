using System.Collections.Generic;
using SlovniFotbal.ViewModels;

namespace SlovniFotbal;

public class Game
{
    public Game(GameType mode, List<string> messages = null, int activePlayerNumber = 1, int seconds = 30)
    {
        this.Messages = messages;
        this.ActivePlayerNumber = activePlayerNumber;
        this.Seconds = seconds;
        this.Mode = mode;
    }
        
    public List<string> Messages { get; set; }
    public int ActivePlayerNumber { get; set; }
    public int Seconds { get; set; }
    public GameType Mode { get; set; }
}