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
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Services.ServiceBus
{
    /// <summary>
    /// False SQL rule filter.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect", Name = "FalseFilter")]
    public sealed class FalseRuleFilter : IRuleFilter, ISqlRuleFilter
    {
        /// <summary>
        /// Gets rule's SQL expression.
        /// </summary>
        [DataMember(Order = 0, Name = "SqlExpression")]
        public string Expression { get; private set; }

        /// <summary>
        /// Compatibility level.
        /// </summary>
        [DataMember(Order = 1)]
        public int CompatibilityLevel { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expression">Rule's SQL expression.</param>
        public FalseRuleFilter(string expression)
        {
            Validator.ArgumentIsNotNullOrEmptyString("expression", expression);

            Expression = expression;
        }
    }
}
