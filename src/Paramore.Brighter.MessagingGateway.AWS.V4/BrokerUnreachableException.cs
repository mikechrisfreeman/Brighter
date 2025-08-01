﻿#region Licence
/* The MIT License (MIT)
Copyright © 2022 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

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

namespace Paramore.Brighter.MessagingGateway.AWS.V4;

/// <summary>
/// When we validated the existence of a topic, it was not found
/// </summary>
public class BrokerUnreachableException : Exception
{
    /// <summary>
    /// No additional data, but the topic was not found
    /// </summary>
    public BrokerUnreachableException() { }
        
    /// <summary>
    /// The topic was not found with additional information
    /// </summary>
    /// <param name="message">What were we trying to do when this happened</param>
    public BrokerUnreachableException(string message) : base(message) { }

    /// <summary>
    /// Another exception prevented us from finding the topic
    /// </summary>
    /// <param name="message">What were we doing when this happened?</param>
    /// <param name="innerException">What was the inner exception</param>
    public BrokerUnreachableException(string message, Exception innerException) : base(message, innerException) { }
}