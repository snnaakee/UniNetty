﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace UniNetty.Codecs.Http
{
    using UniNetty.Buffers;

    public interface IHttpContent : IHttpObject, IByteBufferHolder
    {
    }
}
