using System.Diagnostics;

namespace DemoShared
{
    public class Logger : ILogger
    {
        public void Log(string message)
        {
            Debug.WriteLine($"Logger Dependency: {message}");
        }
    }
}
