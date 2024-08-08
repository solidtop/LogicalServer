using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace LS.Common.Json
{
    public class JsonNamingStrategyProvider(IOptions<ServerOptions> options)
    {
        private readonly JsonOptions _options = options.Value.Json;

        public NamingStrategy GetNamingStrategy()
        {
            return _options.NamingStrategy switch
            {
                JsonNamingStrategy.CamelCase => new CamelCaseNamingStrategy(),
                JsonNamingStrategy.SnakeCase => new SnakeCaseNamingStrategy(),
                JsonNamingStrategy.KebabCase => new KebabCaseNamingStrategy(),
                _ => throw new NotSupportedException($"Unsupported naming strategy: {_options.NamingStrategy}")
            };
        }
    }
}
