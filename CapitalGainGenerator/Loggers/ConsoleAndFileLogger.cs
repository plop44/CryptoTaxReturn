using System;
using System.IO;

namespace CapitalGainGenerator
{
    public class ConsoleAndFileLogger : ILogger
    {
        private readonly string _fileName;

        public ConsoleAndFileLogger(string fileName)
        {
            _fileName = fileName;
        }

        public void LogLine(string line)
        {
            Console.WriteLine(line);
            File.AppendAllText(_fileName, line + Environment.NewLine);
        }

        public void LogHeader(string line)
        {
            Console.WriteLine($"\n-------{line}-------");
            File.AppendAllText(_fileName, $"-------{line}-------" + Environment.NewLine);
        }
    }
}