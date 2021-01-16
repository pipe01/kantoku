using System.Diagnostics;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Kantoku.Master
{
    public class Config
    {
        private string? Path;

        public int RemotePort { get; set; } = 4545;
        public bool UseUglyQR { get; set; }

        public void Save()
        {
            Debug.Assert(Path != null);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            File.WriteAllText(Path, serializer.Serialize(this));
        }

        public static Config Load(string path)
        {
            Config cfg;

            if (File.Exists(path))
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                cfg = deserializer.Deserialize<Config>(File.ReadAllText(path)) ?? new Config();
            }
            else
            {
                cfg = new Config();
            }

            cfg.Path = path;
            cfg.Save();

            return cfg;
        }
    }
}
