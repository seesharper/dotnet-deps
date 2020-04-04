using System;
using Dotnet.Deps.Core;
namespace Dotnet.Deps
{
    public class Program
    {
        public static int Main(string[] args)
        {
            App app = new App(new AppConsole(Console.Out, Console.Error));
            return app.Execute(args);
        }
    }
}
