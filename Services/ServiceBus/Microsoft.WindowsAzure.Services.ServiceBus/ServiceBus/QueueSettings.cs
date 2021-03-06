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
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Services.ServiceBus
{
    /// <summary>
    /// Queue creation options.
    /// </summary>
    [DataContract(Namespace="http://schemas.microsoft.com/netservices/2010/10/servicebus/connect", Name="QueueDescription")]
    public sealed class QueueSettings
    {
        /// <summary>
        /// Determines the amount of time in which a message should be locked 
        /// for processing by a receiver.
        /// </summary>
        [DataMember(Order = 0)]
        public TimeSpan? LockDuration { get; set; }

        /// <summary>
        /// Specifies the maximum queue size in megabytes.
        /// </summary>
        [DataMember(Order = 1, Name = "MaxSizeInMegabytes")]
        public long? MaximumSizeInMegabytes { get; set; }

        /// <summary>
        /// Tells whether duplicate detection is enabled.
        /// </summary>
        [DataMember(Order = 2)]
        public bool? RequiresDuplicateDetection { get; set; }

        /// <summary>
        /// Tells whether the queue is session-aware.
        /// </summary>
        [DataMember(Order = 3)]
        public bool? RequiresSession { get; set; }

        /// <summary>
        /// Specifies default message time to live. 
        /// </summary>
        [DataMember(Order = 4)]
        public TimeSpan? DefaultMessageTimeToLive { get; set; }

        /// <summary>
        /// Tells how the Service Bus controls handles a message whose TTL has
        /// expired.
        /// </summary>
        [DataMember(Order = 5, Name = "DeadLetteringOnMessageExpiration")]
        public bool? EnableDeadLetteringOnMessageExpiration { get; set; }

        /// <summary>
        /// Specifies the time span during which the Service Bus detects 
        /// message duplication.
        /// </summary>
        [DataMember(Order = 6)]
        public TimeSpan? DuplicateDetectionHistoryTimeWindow { get; set; }

        /// <summary>
        /// Specifies the maximum number of times a message SB will try to 
        /// deliver before being dead lettered or discarded.
        /// </summary>
        [DataMember(Order = 7, Name = "MaxDeliveryCount")]
        public int? MaximumDeliveryCount { get; set; }

        /// <summary>
        /// Tells whether server-side batching is enabled.
        /// </summary>
        [DataMember(Order = 8)]
        public bool? EnableBatchedOperations { get; set; }
    }
}
