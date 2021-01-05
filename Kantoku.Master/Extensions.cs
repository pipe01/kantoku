﻿using Serilog;
using System;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Kantoku.Master
{
    public static class LoggerExtensions
    {
        public static ILogger For<T>(this ILogger logger) => logger.ForContext("SourceContext", PrettyName(typeof(T).Name));
        public static ILogger For(this ILogger logger, string name) => logger.ForContext("SourceContext", name);

        private static string PrettyName(string name)
        {
            var str = new StringBuilder();

            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];

                if (char.IsUpper(c) && i > 0)
                {
                    str.Append(' ').Append(char.ToLower(c));
                }
                else
                {
                    str.Append(c);
                }
            }

            return str.ToString();
        }
    }

    public static class JsonExtensions
    {
        public static T? ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
                element.WriteTo(writer);
            return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
        }

        public static T? ToObject<T>(this JsonDocument document, JsonSerializerOptions? options = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            return document.RootElement.ToObject<T>(options);
        }
    }
}
