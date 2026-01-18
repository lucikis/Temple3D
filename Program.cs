using OpenTK.Windowing.Common;
using System;

namespace Temple3D
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game(1920, 1080, "Evadarea din Templul Elementelor"))
            {
                game.WindowState = WindowState.Fullscreen;
                game.UpdateFrequency = 240.0;
                game.Run();
            }
        }
    }
}