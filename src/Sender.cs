using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

public class MessageSender
{
    private readonly string _queueUrl;
    private readonly IAmazonSQS _sqsClient;

    public MessageSender(AWSCredentials credentials, string queueUrl)
    {
        _queueUrl = queueUrl;
        _sqsClient = new AmazonSQSClient(credentials, RegionEndpoint.EUWest2);
    }

    public async Task SendMessages(int numberOfMessages = 5)
    {
        List<SendMessageBatchRequestEntry> messages = [];
        for (int i = 1; i <= numberOfMessages; i++)
        {
            var message = new SendMessageBatchRequestEntry(Guid.NewGuid().ToString(), $"Message {i}");
            messages.Add(message);
        }

        await _sqsClient.SendMessageBatchAsync(new SendMessageBatchRequest {
            QueueUrl = _queueUrl,
            Entries = messages
        });
    }
}