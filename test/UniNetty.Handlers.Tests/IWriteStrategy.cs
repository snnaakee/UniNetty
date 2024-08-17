// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace UniNetty.Handlers.Tests
{
    using System;
    using System.Threading.Tasks;
    using UniNetty.Transport.Channels.Embedded;

    public interface IWriteStrategy
    {
        Task WriteToChannelAsync(IEmbeddedChannel channel, ArraySegment<byte> input);
    }
}