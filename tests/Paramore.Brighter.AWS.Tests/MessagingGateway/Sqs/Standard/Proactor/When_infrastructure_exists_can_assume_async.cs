﻿using System;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Paramore.Brighter.AWS.Tests.Helpers;
using Paramore.Brighter.AWS.Tests.TestDoubles;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.MessagingGateway.AWSSQS;
using Xunit;

namespace Paramore.Brighter.AWS.Tests.MessagingGateway.Sqs.Standard.Proactor;

[Trait("Category", "AWS")]
[Trait("Fragile", "CI")]
public class AWSAssumeInfrastructureTestsAsync  : IDisposable, IAsyncDisposable
{    
    private readonly Message _message;
    private readonly SqsMessageConsumer _consumer;
    private readonly SqsMessageProducer _messageProducer;
    private readonly ChannelFactory _channelFactory;
    private readonly MyCommand _myCommand;

    public AWSAssumeInfrastructureTestsAsync()
    {
        _myCommand = new MyCommand{Value = "Test"};
        const string replyTo = "http:\\queueUrl";
        var contentType = new ContentType(MediaTypeNames.Text.Plain);
        var correlationId = Guid.NewGuid().ToString();
        var subscriptionName = $"Producer-Send-Tests-{Guid.NewGuid().ToString()}".Truncate(45);
        var queueName = $"Producer-Send-Tests-{Guid.NewGuid().ToString()}".Truncate(45);
        var routingKey = new RoutingKey(queueName);
        var channelName = new ChannelName(queueName);
        
        var subscription = new SqsSubscription<MyCommand>(
            subscriptionName: new SubscriptionName(subscriptionName),
            channelName: channelName,
            channelType: ChannelType.PointToPoint, 
            routingKey: routingKey, 
            messagePumpType: MessagePumpType.Proactor,
            makeChannels: OnMissingChannel.Create);
            
        _message = new Message(
            new MessageHeader(_myCommand.Id, routingKey, MessageType.MT_COMMAND, correlationId: correlationId, 
                replyTo: new RoutingKey(replyTo), contentType: contentType),
            new MessageBody(JsonSerializer.Serialize((object) _myCommand, JsonSerialisationOptions.Options))
        );

        var awsConnection = GatewayFactory.CreateFactory();
            
        //We need to do this manually in a test - will create the channel from subscriber parameters
        //This doesn't look that different from our create tests - this is because we create using the channel factory in
        //our AWS transport, not the consumer (as it's a more likely to use infrastructure declared elsewhere)
        _channelFactory = new ChannelFactory(awsConnection);
        var channel = _channelFactory.CreateAsyncChannel(subscription);
            
        //Now change the subscription to validate, just check what we made
        subscription.MakeChannels = OnMissingChannel.Assume;
            
        _messageProducer = new SqsMessageProducer(
            awsConnection, 
            new SqsPublication(channelName: channel.Name, makeChannels: OnMissingChannel.Assume)
            );

        _consumer = new SqsMessageConsumer(awsConnection, channel.Name.ToValidSQSQueueName());
    }

    [Fact]
    public async Task When_infastructure_exists_can_assume()
    {
        //arrange
        await _messageProducer.SendAsync(_message);
            
        var messages = await _consumer.ReceiveAsync(TimeSpan.FromMilliseconds(5000));
            
        //Assert
        var message = messages.First();
        Assert.Equal(_myCommand.Id, message.Id);

        //clear the queue
        await _consumer.AcknowledgeAsync(message);
    }
 
    public void Dispose()
    {
        //Clean up resources that we have created
        _channelFactory.DeleteTopicAsync().Wait();
        _channelFactory.DeleteQueueAsync().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await _channelFactory.DeleteTopicAsync();
        await _channelFactory.DeleteQueueAsync();
    }
}
