using OpenTK.Windowing.Common;
using System;

namespace Temple3D
{
    class Program
    {
        static void Main(string[] args)
        {
            // Rulează jocul la rezoluția 800x600
            using (Game game = new Game(1920, 1080, "Evadarea din Templul Elementelor"))
            {
                game.WindowState = WindowState.Fullscreen;
                // Setăm FPS maxim (pentru a nu solicita GPU-ul la 1000 FPS inutil)
                game.UpdateFrequency = 240.0;
                game.Run();
            }
        }
    }
}