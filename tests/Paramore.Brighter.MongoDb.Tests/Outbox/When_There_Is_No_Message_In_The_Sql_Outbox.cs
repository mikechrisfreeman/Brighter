#region Licence

/* The MIT License (MIT)
Copyright © 2014 Francesco Pighi <francesco.pighi@gmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System;
using Paramore.Brighter.Outbox.MongoDb;
using Xunit;

namespace Paramore.Brighter.MongoDb.Tests.Outbox;

[Trait("Category", "MongoDb")]
public class MongoDbOutboxEmptyStoreTests : IDisposable
{
    private readonly string _collection;
    private readonly Message _messageEarliest;
    private readonly MongoDbOutbox _outbox;

    public MongoDbOutboxEmptyStoreTests()
    {
        _collection = $"outbox-{Guid.NewGuid():N}";
        _outbox = new (Configuration.CreateOutbox(_collection));
        _messageEarliest = new Message(
            new MessageHeader(Guid.NewGuid().ToString(), new RoutingKey("test_topic"), MessageType.MT_DOCUMENT), 
            new MessageBody("message body")
        );
    }

    [Fact]
    public void When_There_Is_No_Message_In_The_MongoDb_Outbox()
    {
        var storedMessage = _outbox.Get(_messageEarliest.Id, new RequestContext());

        //should return a empty message
        Assert.Equal(MessageType.MT_NONE, storedMessage.Header.MessageType);
    }

    public void Dispose()
    {
        Configuration.Cleanup(_collection);
    }
}
