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
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Services.ServiceBus;

using NetFormUrlEncodedContent = System.Net.Http.FormUrlEncodedContent;

namespace Microsoft.WindowsAzure.Services.ServiceBus.Http
{
    /// <summary>
    /// HTTP handler for WRAP authentication of outgoing requests.
    /// </summary>
    /// <remarks>This class uses handlers down the chain for sending its
    /// authentication requests. This is why it requires specifying another
    /// HTTP in the constructor.</remarks>
    public sealed class WrapAuthenticationHandler : IHttpHandler
    {
        private HttpChannel _channel;                                   // Channel for processing authentication requests.
        private Dictionary<string, WrapToken> _tokens;                  // Cached tokens.
        private Object _syncObject;                                     // Synchronization object for accessing cached tokens.

        private string _namespace;                                      // Service namespace.
        private string _issuerName;                                     // Issuer name.
        private string _issuerPassword;                                 // Password.
        private Uri _authenticationUri;                                 // URI for processing authentication requests.
        private Uri _serviceHostUri;                                    // Host part of the URI for accessing service resources.

        /// <summary>
        /// Initializes WARP authentication handler for use in Service Bus.
        /// </summary>
        /// <param name="serviceNamespace">Namespace.</param>
        /// <param name="issuerName">User name.</param>
        /// <param name="issuerPassword">Password.</param>
        public WrapAuthenticationHandler(string serviceNamespace, string issuerName, string issuerPassword)
        {
            Validator.ArgumentIsValidPath("serviceNamespace", serviceNamespace);
            Validator.ArgumentIsNotNullOrEmptyString("issuerName", issuerName);
            Validator.ArgumentIsNotNull("issuerPassword", issuerPassword);

            _channel = new HttpChannel();
            _namespace = serviceNamespace;
            _issuerName = issuerName;
            _issuerPassword = issuerPassword;
            _channel = new HttpChannel();
            _syncObject = new object();
            _tokens = new Dictionary<string, WrapToken>(StringComparer.OrdinalIgnoreCase);

            string uriString = string.Format(CultureInfo.InvariantCulture, Constants.ServiceBusAuthenticationUri, serviceNamespace);
            _authenticationUri = new Uri(uriString, UriKind.Absolute);

            uriString = string.Format(CultureInfo.InvariantCulture, Constants.ServiceBusScopeUri, serviceNamespace);
            _serviceHostUri = new Uri(uriString, UriKind.Absolute);
        }

        /// <summary>
        /// Disposes the handler.
        /// </summary>
        public void Dispose()
        {
            _channel.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Processes the request by updating it with the authentication data.
        /// </summary>
        /// <param name="request">Request to process.</param>
        /// <returns>Processed request.</returns>
        HttpRequest IHttpHandler.ProcessRequest(HttpRequest request)
        {
            Validator.ArgumentIsNotNull("request", request);

            WrapToken token = GetToken(request.Uri.AbsolutePath);
            token.Authorize(request);
            return request;
        }

        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="response">Response to process.</param>
        /// <returns>Processed response.</returns>
        HttpResponse IHttpHandler.ProcessResponse(HttpResponse response)
        {
            Validator.ArgumentIsNotNull("response", response);

            return response;
        }

        /// <summary>
        /// Gets authentication token for a resource with the given path.
        /// </summary>
        /// <param name="resourcePath">Resource path.</param>
        /// <returns>Authentication token.</returns>
        private WrapToken GetToken(string resourcePath)
        {
            WrapToken token;

            lock (_syncObject)
            {
                _tokens.TryGetValue(resourcePath, out token);
                if (token != null && token.IsExpired)
                {
                    _tokens.Remove(resourcePath);
                }
            }

            if (token == null)
            {
                // Issue new authentication request.
                Uri scopeUri = new Uri(_serviceHostUri, resourcePath);
                HttpRequest request = new HttpRequest(HttpMethod.Post, _authenticationUri);
                Dictionary<string, string> settings = new Dictionary<string, string>()
                {
                    {"wrap_name",       _issuerName},
                    {"wrap_password",   _issuerPassword},
                    {"wrap_scope",      scopeUri.ToString()},
                };

                request.Headers.Add(Constants.AcceptHeader, Constants.WrapAuthenticationContentType);

                // Encode the content
                NetFormUrlEncodedContent netContent = new NetFormUrlEncodedContent(settings);
                request.Content = HttpContent.CreateFromByteArray(netContent.ReadAsByteArrayAsync().Result);

                HttpResponse response = _channel.SendAsyncInternal(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new WrapAuthenticationException(response);
                }
                token = new WrapToken(resourcePath, response);

                lock (_syncObject)
                {
                    WrapToken existingToken;

                    if (_tokens.TryGetValue(resourcePath, out existingToken) && !existingToken.IsExpired)
                    {
                        // Ignore new results; use existing token
                        token = existingToken;
                    }
                    else
                    {
                        // Cache the token
                        _tokens[resourcePath] = token;
                    }
                }
            }

            return token;
        }
    }
}
