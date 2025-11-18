using System;
using System.Runtime.InteropServices;

namespace CubeSurvivor
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            // Criar um console nativo no Windows (projeto usa OutputType WinExe)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                AllocConsole();
            }

            Console.WriteLine("=== INICIANDO CUBE SURVIVOR ===");
            Console.WriteLine($"Diretório atual: {AppDomain.CurrentDomain.BaseDirectory}");
            
            try
            {
                Console.WriteLine("Criando instância do jogo...");
                using (var game = new Game1())
                {
                    Console.WriteLine("Executando jogo...");
                    game.Run();
                    Console.WriteLine("Jogo encerrado normalmente.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n!!! ERRO FATAL !!!");
                Console.WriteLine($"Tipo: {e.GetType().Name}");
                Console.WriteLine($"Mensagem: {e.Message}");
                Console.WriteLine($"\nStack Trace:\n{e.StackTrace}");
                
                if (e.InnerException != null)
                {
                    Console.WriteLine($"\n--- Inner Exception ---");
                    Console.WriteLine($"Tipo: {e.InnerException.GetType().Name}");
                    Console.WriteLine($"Mensagem: {e.InnerException.Message}");
                    Console.WriteLine($"Stack Trace:\n{e.InnerException.StackTrace}");
                }
                
                Console.WriteLine("\nPressione ENTER para fechar...");
                Console.ReadLine();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();
    }
}