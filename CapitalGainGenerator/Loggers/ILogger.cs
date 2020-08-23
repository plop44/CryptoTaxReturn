namespace CapitalGainGenerator
{
    public interface ILogger
    {
        void LogLine(string line);
        void LogHeader(string line);
    }
}