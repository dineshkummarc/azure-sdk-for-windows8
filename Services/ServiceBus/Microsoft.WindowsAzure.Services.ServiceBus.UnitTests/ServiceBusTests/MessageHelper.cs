﻿//
// Copyright 2012 Microsoft Corporation
// 
// Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Services.ServiceBus;
using Microsoft.WindowsAzure.Services.ServiceBus.Http;

namespace Microsoft.WindowsAzure.Services.ServiceBus.UnitTests.ServiceBusTests
{
    /// <summary>
    /// Helper class for dealing with messages.
    /// </summary>
    internal static class MessageHelper
    {
        /// <summary>
        /// Creates a brokered message with the text content.
        /// </summary>
        /// <param name="messageText">Message text.</param>
        /// <returns>Brokered message.</returns>
        internal static BrokeredMessageSettings CreateMessage(string messageText, string contentType = "text/plain")
        {
            return BrokeredMessageSettings.CreateFromText(messageText, contentType);
        }

        /// <summary>
        /// Creates a brokered message with the binary content.
        /// </summary>
        /// <param name="bytes">Message content.</param>
        /// <returns>Brokered message.</returns>
        internal static BrokeredMessageSettings CreateMessage(byte[] bytes)
        {
            return BrokeredMessageSettings.CreateFromByteArray(bytes);
        }

        /// <summary>
        /// Creates a brokered message with the binary content.
        /// </summary>
        /// <param name="stream">Message content.</param>
        /// <returns>Brokered message.</returns>
        internal static BrokeredMessageSettings CreateMessage(Stream stream)
        {
            return BrokeredMessageSettings.CreateFromStream(stream.AsInputStream());
        }
    }
}
