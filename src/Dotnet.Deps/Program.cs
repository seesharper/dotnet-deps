using System;
using Dotnet.Deps.Core;
namespace Dotnet.Deps
{
    public class Program
    {
        public static void Main(string[] args)
        {
            App app = new App(new AppConsole(Console.Out, Console.Error));
            app.Execute(args);
        }
    }
}
