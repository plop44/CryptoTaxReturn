using System;
using System.IO;
using System.Linq;
using Common.Files;

namespace Common.Rest
{
    public class BinanceKeyAndSecret : IFileRepositoryConfig
    {
        public BinanceKeyAndSecret(PathsConfig apiKeyConfig)
        {
            if (!File.Exists(apiKeyConfig.ApiKeyConfigPath)) throw new ArgumentException($"no api key file existing at {apiKeyConfig.ApiKeyConfigPath}.");

            var readLines = File.ReadLines(apiKeyConfig.ApiKeyConfigPath).ToArray();

            if (readLines.Length != 3 )
                throw new Exception("Binance api Key file not valid, expecting 3 lines: key, secret, tag. Maybe you are targeting a KuCoin Api Key?");

            Key = readLines[0];
            Secret = readLines[1];
            Tag = readLines[2];
        }

        public string Key { get; }
        public string Secret { get; }
        public string Tag { get; }
        public string Account => Tag;
    }
}