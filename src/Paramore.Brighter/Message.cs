﻿#region Licence
/* The MIT License (MIT)
Copyright © 2014 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

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
using System.Net.Mime;
using System.Text.Json.Serialization;

namespace Paramore.Brighter
{
    /// <summary>
    /// A message sent over <a href="http://parlab.eecs.berkeley.edu/wiki/_media/patterns/taskqueue.pdf">Task Queue</a> for asynchronous processing of a <see cref="Command"/>
    /// or <see cref="Event"/>
    /// </summary>
    public class Message : IEquatable<Message>
    {
        public const string OriginalMessageIdHeaderName = "x-original-message-id";
        /// <summary>
        /// Tag name for the delivery tag header   
        /// </summary>
        public const string DeliveryTagHeaderName = "DeliveryTag";
        
        /// <summary>
        /// Tag name for the redelivered header
        /// </summary>
        public const string RedeliveredHeaderName = "Redelivered";

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>The header.</value>
        public MessageHeader Header { get; init; }
        
        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <value>The body.</value>
        public MessageBody Body { get; set; }

        /// <summary>
        /// RMQ: An identifier for the message set by the broker. Only valid on the same thread that consumed the message.
        /// </summary>
        [JsonIgnore]
        public ulong DeliveryTag
        {
            get
            {
                if (Header.Bag.TryGetValue(DeliveryTagHeaderName, out object? value))
                    return (ulong) value;
                else
                    return 0;
            }
            set { Header.Bag[DeliveryTagHeaderName] = value; }
        }
        
        public static Message Empty => new();
        
        /// <summary>
        /// Returns true if this is an empty Message.
        /// </summary>
        public bool IsEmpty => Header.MessageType == MessageType.MT_NONE;

        /// <summary>
        /// Gets the identifier of the message.
        /// </summary>
        /// <value>The identifier.</value>
        public Id Id => Header.MessageId;

        /// <summary>
        /// RMQ: Is the message persistent
        /// </summary>
        [JsonIgnore]
        public bool Persist { get; set; }
        
        /// <summary>
        /// RMQ: Has this message been redelivered
        /// </summary>
        [JsonIgnore]
        public bool Redelivered
        {
            get
            {
                if (Header.Bag.TryGetValue(RedeliveredHeaderName, out object? value))
                    return (bool) value;
                else
                {
                    return false;
                }
            }
            set { Header.Bag[RedeliveredHeaderName] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message()
        {
            Header = new MessageHeader(messageId: string.Empty, topic: RoutingKey.Empty, messageType: MessageType.MT_NONE);
            Body = new MessageBody(string.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        [JsonConstructor]
        public Message(MessageHeader header, MessageBody body)
        {
            Body = body;
            Header = header;
            var contentType = Header.ContentType;
            if (contentType is null && Body.ContentType is not null)
                // If the header does not have a content type, but the body does, then we set the header to the body content type
                contentType = Body.ContentType;
            else if (contentType is null)
                //otherwise we just default to text/plain 
                contentType = new ContentType(MediaTypeNames.Text.Plain);    
            Header.ContentType = contentType;
        }

        /// <summary>
        /// Determines if the message has been requeued a number of times greater than a threshold
        /// </summary>
        /// <remarks>Generally used to send the message to a DLQ to prevent a poision pill</remarks>
        /// <param name="requeueCount">The threshold to determine if the count exceeds</param>
        /// <returns></returns>
        public bool HandledCountReached(int requeueCount)
        {
            return Header.HandledCount >= requeueCount;
        }

        /// <summary>
        /// Propogates the trace context for the message, when being sent across a trace boundary.
        /// We set this value in the headers of the message
        /// </summary>
        /// <param name="message">The message to set the trace context into</param>
        /// <param name="key">A key representing the value to set</param>
        /// <param name="value">The value to set </param>
        public static void PropogateContext(Message message, string key, string? value)
        {
            if (value is null)
                return;
            
            switch (key)
            {
                case "traceparent":
                    message.Header.TraceParent = value;
                    break;
                case "tracestate":
                    message.Header.TraceState = value;
                    break;
                case "baggage":
                    message.Header.Baggage.LoadBaggage(value);
                    break;
            }
        }
        
        /// <summary>
        /// Creates a failure message, used to indicate that a message could not be successfully retrieved from a channel
        /// </summary>
        /// <param name="topic">The topic of the channel</param>
        /// <param name="messageId">The id of he message, may be Id.Empty if not known</param>
        /// <returns></returns>
        public static Message FailureMessage(RoutingKey? topic, Id? messageId = null)
        {
            var header = MessageHeader.FailureMessageHeader(topic, messageId);
            var message = new Message(header, new MessageBody(string.Empty));
            return message;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(Message? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Header.Equals(other.Header) && Body.Equals(other.Body);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Message)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Header.GetHashCode() * 397) ^ (Body is not null ? Body.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Implements the ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Message? left, Message? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Message? left, Message? right)
        {
            return !Equals(left, right);
        }
    }
}
