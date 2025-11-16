using System;

namespace CubeSurvivor
{
    /// <summary>
    /// Ponto de entrada do programa
    /// </summary>
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
}
