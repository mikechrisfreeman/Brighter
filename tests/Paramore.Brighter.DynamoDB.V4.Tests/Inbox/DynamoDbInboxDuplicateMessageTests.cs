﻿#region Licence
/* The MIT License (MIT)
Copyright © 2015 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

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
using Paramore.Brighter.DynamoDB.V4.Tests.TestDoubles;
using Paramore.Brighter.Inbox.DynamoDB.V4;
using Xunit;

namespace Paramore.Brighter.DynamoDB.V4.Tests.Inbox;

[Trait("Category", "DynamoDB")]
public class DynamoDbInboxDuplicateMessageTests : DynamoDBInboxBaseTest
{
    private readonly DynamoDbInbox _dynamoDbInbox;
    private readonly string _contextKey;
    private readonly MyCommand _raisedCommand;

    private Exception _exception;

    public DynamoDbInboxDuplicateMessageTests()
    {
        _dynamoDbInbox = new DynamoDbInbox(Client, new DynamoDbInboxConfiguration());
        _raisedCommand = new MyCommand { Value = "Test" };
        _contextKey = "context-key";
        _dynamoDbInbox.Add(_raisedCommand, _contextKey, null);
    }

    [Fact]
    public void When_The_Message_Is_Already_In_The_Inbox()
    {
        _exception = Catch.Exception(() => _dynamoDbInbox.Add(_raisedCommand, _contextKey, null));

        //_should_succeed_even_if_the_message_is_a_duplicate
        Assert.Null(_exception);
    }

    [Fact]
    public void When_The_Message_Is_Already_In_The_Inbox_Different_Context()
    {
        _dynamoDbInbox.Add(_raisedCommand, "some other key", null);

        var storedCommand = _dynamoDbInbox.Get<MyCommand>(_raisedCommand.Id, "some other key", null);

        //_should_read_the_command_from_the__dynamo_db_inbox
        Assert.NotNull(storedCommand);
    }
}
