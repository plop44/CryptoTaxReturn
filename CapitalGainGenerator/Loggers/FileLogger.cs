using System;
using System.IO;

namespace CapitalGainGenerator
{
    public class FileLogger : ILogger
    {
        private readonly string _fileName;

        public FileLogger(string fileName)
        {
            _fileName = fileName;
        }

        public void LogLine(string line)
        {
            File.AppendAllText(_fileName, line + Environment.NewLine);
        }

        public void LogHeader(string line)
        {
            File.AppendAllText(_fileName, $"-------{line}-------" + Environment.NewLine);
        }
    }
}