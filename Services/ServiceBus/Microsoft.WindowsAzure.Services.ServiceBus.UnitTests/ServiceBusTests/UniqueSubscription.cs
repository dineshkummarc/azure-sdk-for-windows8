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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Services.ServiceBus.UnitTests.ServiceBusTests
{
    /// <summary>
    /// Class for generating unique topic/subscription pair.
    /// </summary>
    internal class UniqueSubscription: UniqueTopic
    {
        /// <summary>
        /// Gets the subscription name.
        /// </summary>
        internal string SubscriptionName { get; private set; }

        /// <summary>
        /// Creates a unique subscription.
        /// </summary>
        internal UniqueSubscription()
            : base()
        {
            SubscriptionName = Configuration.GetUniqueSubscriptionName();
            Configuration.ServiceBus.CreateSubscriptionAsync(TopicName, SubscriptionName).AsTask().Wait();
        }
    }
}
