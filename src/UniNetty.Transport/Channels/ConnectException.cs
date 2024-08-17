﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace UniNetty.Transport.Channels
{
    using System;

    public class ConnectException : Exception
    {
        public ConnectException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}