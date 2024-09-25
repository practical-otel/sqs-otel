// See https://aka.ms/new-console-template for more information

using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var configBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json");
var configuration = configBuilder.Build();

if (!new CredentialProfileStoreChain()
    .TryGetAWSCredentials(configuration["AWS:Profile"], out var credentialProfile))
{
    Console.WriteLine("Unable to get credentials");
    return;
}

var queueUrl = configuration["AWS:QueueUrl"];
if (string.IsNullOrEmpty(queueUrl))
{
    Console.WriteLine("No Queue Url set");
    return;
}
else
{
    Console.WriteLine($"Queue Url: {queueUrl}");
}

var receiver = new MessageReceiver(credentialProfile, queueUrl);
var sender = new MessageSender(credentialProfile, queueUrl);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton(receiver);
        services.AddHostedService<MessageReceiverBackgroundService>();
    })
    .ConfigureLogging(logging => logging.ClearProviders());



await host.StartAsync();

await Task.Run(() => sender.SendMessages(5));

Console.WriteLine("Press any key to end the application");
Console.ReadKey();