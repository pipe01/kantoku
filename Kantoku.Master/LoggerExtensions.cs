using Serilog;
using System.Text;

namespace Kantoku.Master
{
    public static class LoggerExtensions
    {
        public static ILogger For<T>(this ILogger logger) => logger.ForContext("SourceContext", PrettyName(typeof(T).Name));

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
}
