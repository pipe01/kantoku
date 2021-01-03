using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Kantoku.Master
{
    public class Config
    {
        public record Provider(string Type, string? Name);

        public Provider[]? Providers { get; set; }
        public object? Test { get; set; }

        private Config() { }

        public static Config Load(string path)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<Config>(File.ReadAllText(path));
        }
    }
}
