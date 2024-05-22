using NBomber.CSharp;
using NBomber.Http;
using NBomber.Http.CSharp;
using API.Load.Tests.Models.Requests;
using API.Load.Tests.Models.Responses;
using API.Load.Tests.Builders;
using NBomber.Data.CSharp;
using NBomber.Data;
using API.Load.Tests.Data;
using NBomber.Contracts;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using Newtonsoft.Json;

namespace API.Load.Tests
{
    public class Tests
    {
        private IDataFeed<CreateUserRequest> _usersFeed;

        private const string _baseUrl = "http://localhost:5000/api/";

        private const string _scenarioName = "Example load test";
        private const string _registerStepName = "register";
        private const string _loginStepName = "login";
        private const string _getProductsStepName = "get products";

        [Test]
        public void Example_LoadTest()
        {
            // Arrange
            using var httpClient = new HttpClient();

            Http.GlobalJsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var scenario = Scenario.Create(_scenarioName, async context =>
            {
                var fakeUser = _usersFeed.GetNextItem(context.ScenarioInfo);

                var registerStep = await Step.Run(_registerStepName, context, async () =>
                {
                    var request = RequestBuilder.CreatePostRequest(_baseUrl + "v1/users", fakeUser);

                    var response = await Http.Send(httpClient, HttpClientArgsBuilder.Create(context), request);
                    return response;
                });

                var loginStep = await Step.Run(_loginStepName, context, async () =>
                {
                    var user = new LoginRequest { UserName = fakeUser.UserName, Password = fakeUser.Password };

                    var request = RequestBuilder.CreatePostRequest(_baseUrl + "v1/account/login", user);

                    var response = await Http.Send<TokenInfo>(httpClient, HttpClientArgsBuilder.Create(context), request);
                    return response;
                });

                var getProductsStep = await Step.Run(_getProductsStepName, context, async () =>
                {
                    var request = RequestBuilder.CreateGetRequest(_baseUrl + "v1/products");
                    if (loginStep.Payload.IsSome())
                    {
                        var accessToken = JsonConvert.DeserializeObject<TokenInfo>(loginStep.Payload.Value.Data.ToString() ?? string.Empty);
                        if (accessToken != null)
                        {
                            request.Authorize(accessToken.AccessToken);
                        }
                        else
                        {
                            context.Logger.Information($"Token was null for user: {fakeUser.UserName}");
                        }
                    }

                    var response = await Http.Send<ProductResponse>(httpClient, HttpClientArgs.Create(logger: context.Logger), request);
                    return response;
                });

                return Response.Ok();
            }).WithInit(ctx =>
            {
                ctx.Logger.Information("MY INIT");
                // We crate 300 users 
                var users = FakeUsersGenerator.GenerateFakeUsers(300).ToArray();

                _usersFeed = DataFeed.Circular(users);

                return Task.CompletedTask;
            }).WithClean(ctx =>
            {
                ctx.Logger.Information("MY CLEAN");

                return Task.CompletedTask;
            }).WithWarmUpDuration(TimeSpan.FromSeconds(5))
              .WithLoadSimulations(Simulation.Inject(
                rate: 5, 
                interval: TimeSpan.FromSeconds(1), 
                during: TimeSpan.FromSeconds(10)));

            // Act
            var result = NBomberRunner
                .RegisterScenarios(scenario)
                //.LoadConfig("./config.json")
                .Run();

            // Assert
            using (new AssertionScope())
            {
                var scnStats = result.ScenarioStats.Get(_scenarioName);

                var registerStepStats = scnStats.StepStats.Get(_registerStepName);
                var loginStepStats = scnStats.StepStats.Get(_loginStepName);
                var getProductsStepStats = scnStats.StepStats.Get(_getProductsStepName);

                // Scenario
                scnStats.Ok.Request.RPS.Should().BeGreaterThan(0);       // request per second > 0
                scnStats.Ok.Latency.Percent75.Should().BeLessThan(1100); // 75% of requests below 1100ms.
                scnStats.Ok.Latency.Percent99.Should().BeLessThan(1300); // 99% of requests below 1300ms.

                scnStats.Fail.Request.Percent.Should().BeLessThan(1);   // less than 1% of errors

                // Steps
                registerStepStats.Ok.Request.Percent.Should().Be(100); // success rate 100% of all requests
                registerStepStats.Fail.Request.Percent.Should().Be(0);

                loginStepStats.Ok.Request.Percent.Should().Be(100); // success rate 100% of all requests
                loginStepStats.Fail.Request.Percent.Should().Be(0);

                getProductsStepStats.Ok.Request.Percent.Should().Be(100); // success rate 100% of all requests
                getProductsStepStats.Fail.Request.Percent.Should().Be(0);
            }
        }
    }
}