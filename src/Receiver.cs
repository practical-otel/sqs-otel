using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;

public class MessageReceiverBackgroundService : BackgroundService
{
    private readonly MessageReceiver _messageReceiver;
    public MessageReceiverBackgroundService(MessageReceiver messageReceiver)
    {
        _messageReceiver = messageReceiver;

    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageReceiver.Start();
    }
}

public class MessageReceiver
{
    private readonly string _queueUrl;
    private readonly IAmazonSQS _sqsClient;

    public MessageReceiver(AWSCredentials credentials, string queueUrl)
    {
        _queueUrl = queueUrl;
        _sqsClient = new AmazonSQSClient(credentials, RegionEndpoint.EUWest2);
    }

    public async Task Start()
    {
        while(true)
        {
            var receiveResponse = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest{
                QueueUrl=_queueUrl,
                MaxNumberOfMessages=10,
                WaitTimeSeconds=20,
                VisibilityTimeout=20
            });
            foreach (var message in receiveResponse.Messages)
            {
                await ProcessMessage(message);
                await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest{
                    QueueUrl=_queueUrl,
                    ReceiptHandle=message.ReceiptHandle
                });
            }
        }
    }

    private static Task ProcessMessage(Message message)
    {
        Console.WriteLine($"Recevied Message: {message.Body}");
        return Task.CompletedTask;
    }
}