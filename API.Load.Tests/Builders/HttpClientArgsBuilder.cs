using NBomber.Contracts;
using NBomber.Http;
using System.Text.Json;

namespace API.Load.Tests.Builders
{
    internal static class HttpClientArgsBuilder
    {
        public static HttpClientArgs Create(IScenarioContext context)
        {
            using var timeout = new CancellationTokenSource();
            timeout.CancelAfter(45); // The operation will be canceled after 45 ms

            return HttpClientArgs.Create(
                        timeout.Token,
                        httpCompletion: HttpCompletionOption.ResponseHeadersRead,
                        jsonOptions: new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        },
                        logger: context.Logger
                    );
        }
    }
}
