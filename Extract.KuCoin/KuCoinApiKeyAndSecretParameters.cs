using System;
using System.IO;
using System.Linq;
using Common;
using Common.Files;

namespace Extract.KuCoin
{
    public class KuCoinKeyAndSecret : IFileRepositoryConfig
    {
        public KuCoinKeyAndSecret(PathsConfig apiKeyConfig)
        {
            if (!File.Exists(apiKeyConfig.ApiKeyConfigPath)) throw new ArgumentException($"no api key file existing at {apiKeyConfig.ApiKeyConfigPath}.");

            var readLines = File.ReadLines(apiKeyConfig.ApiKeyConfigPath).ToArray();

            if (readLines.Length < 4)
                throw new Exception("KuCoin Api Key file not valid, expecting 4 lines: key, secret, tag and passPhrase. Maybe you are targeting a Binance Api Key?");

            Key = readLines[0];
            Secret = readLines[1];
            Tag = readLines[2];
            PassPhrase = readLines[3];
        }

        public string Key { get; }
        public string Secret { get; }
        public string Tag { get; }
        public string PassPhrase { get; }
        public string Account => Tag;
    }
}