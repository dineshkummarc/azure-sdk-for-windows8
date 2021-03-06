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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Services.ServiceBus.Http;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Microsoft.WindowsAzure.Services.ServiceBus.UnitTests.HttpTests
{
    /// <summary>
    /// Tests for the Content class.
    /// </summary>
    [TestClass]
    public class ContentTests
    {
        /// <summary>
        /// Creates a memory content from the given text.
        /// </summary>
        /// <param name="text">Content text.</param>
        /// <returns>Memory-initialized content with the given text.</returns>
        private static HttpContent CreateMemoryContent(string text)
        {
            return HttpContent.CreateFromText(text, "text/plain");
        }

        /// <summary>
        /// Creates a memory content from the given bytes.
        /// </summary>
        /// <param name="bytes">Content data.</param>
        /// <returns>Memory-initialized content with the given bytes.</returns>
        private static HttpContent CreateMemoryContent(params byte[] bytes)
        {
            return HttpContent.CreateFromByteArray(bytes);
        }

        /// <summary>
        /// Creates a stream content from the array of bytes.
        /// </summary>
        /// <param name="bytes">Content bytes.</param>
        /// <returns>Stream-initialized content with the given bytes.</returns>
        private static HttpContent CreateStreamContent(params byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            return HttpContent.CreateFromStream(stream.AsInputStream());
        }

        /// <summary>
        /// Creates a stream content from the given string.
        /// </summary>
        /// <param name="text">Content string.</param>
        /// <returns>Stream-initialized text content.</returns>
        private static HttpContent CreateStreamContent(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            MemoryStream stream = new MemoryStream(bytes);
            return HttpContent.CreateFromStream(stream.AsInputStream());
        }


        /// <summary>
        /// Tests passing invalid arguments into content's constructors.
        /// </summary>
        [TestMethod]
        public void InvalidArgs()
        {
            Assert.ThrowsException<ArgumentNullException>(() => HttpContent.CreateFromText(null, "text/plain"));
            Assert.ThrowsException<ArgumentNullException>(() => HttpContent.CreateFromText("test", null));
            Assert.ThrowsException<ArgumentException>(() => HttpContent.CreateFromText("text", ""));
            Assert.ThrowsException<ArgumentNullException>(() => HttpContent.CreateFromByteArray(null));
            Assert.ThrowsException<ArgumentNullException>(() => HttpContent.CreateFromStream(null));

            HttpContent content = HttpContent.CreateFromText("this is a test.", "text/plain");
            Assert.ThrowsException<ArgumentNullException>(() => content.CopyToAsync(null));
        }

        /// <summary>
        /// Tests reading text content as a string.
        /// </summary>
        [TestMethod]
        public void ReadTextAsString()
        {
            string originalContent = Guid.NewGuid().ToString();
            HttpContent content = CreateMemoryContent(originalContent);

            // Must be able to read multiple times
            for (int i = 0; i < 2; i++)
            {
                string readContent = content.ReadAsStringAsync().AsTask().Result;
                Assert.AreEqual(originalContent, readContent, false, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Tests reading text content as a bytes array.
        /// </summary>
        [TestMethod]
        public void ReadTextAsBytes()
        {
            string originalContent = Guid.NewGuid().ToString();
            HttpContent content = HttpContent.CreateFromText(originalContent, "text/plain");

            // Must be able to read multiple times.
            for (int i = 0; i < 2; i++)
            {
                List<byte> bytes = new List<byte>(content.ReadAsByteArrayAsync().AsTask().Result);

                string readContent = Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count);
                Assert.AreEqual(originalContent, readContent, false, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Tests reading text content as a bytes stream.
        /// </summary>
        [TestMethod]
        public void ReadTextAsStream()
        {
            string originalContent = Guid.NewGuid().ToString();
            HttpContent content = HttpContent.CreateFromText(originalContent, "text/plain");

            // Must be able to read multiple times.
            for (int i = 0; i < 2; i++)
            {
                Stream stream = content.ReadAsStreamAsync().AsTask().Result.AsStreamForRead();
                using (StreamReader reader = new StreamReader(stream))
                {
                    string readContent = reader.ReadToEnd();
                    Assert.AreEqual(originalContent, readContent, false, CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Tests buffering memory content (which is a no-op operation).
        /// </summary>
        [TestMethod]
        public void BufferMemoryContent()
        {
            string originalContent = Guid.NewGuid().ToString();
            HttpContent content = CreateMemoryContent(originalContent);

            content.CopyToBufferAsync().AsTask().Wait();

            // Must be able to read as many times as we want.
            for (int i = 0; i < 2; i++)
            {
                string readContent = content.ReadAsStringAsync().AsTask().Result;
                Assert.AreEqual(originalContent, readContent, false, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Tests copying memory content into a stream.
        /// </summary>
        [TestMethod]
        public void CopyMemoryContent()
        {
            byte[] originalContent = new byte[] { 1, 2, 3, 4, };
            HttpContent content = CreateMemoryContent(originalContent);

            // Memory content allows multiple reads.
            for (int i = 0; i < 2; i++)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    content.CopyToAsync(stream.AsOutputStream()).AsTask().Wait();
                    stream.Flush();
                    byte[] readContent = stream.ToArray();

                    TestHelper.Equal(originalContent, readContent);
                }
            }
        }

        /// <summary>
        /// Tests reading stream content as a string.
        /// </summary>
        [TestMethod]
        public void ReadStreamAsText()
        {
            string originalContent = Guid.NewGuid().ToString();
            HttpContent content = CreateStreamContent(originalContent);

            // Reading the first time should be OK.
            string readContent = content.ReadAsStringAsync().AsTask().Result;
            Assert.AreEqual(originalContent, readContent, false, CultureInfo.InvariantCulture);

            // Reading again: there should be nothing left in the stream.
            readContent = content.ReadAsStringAsync().AsTask().Result;
            Assert.AreEqual(readContent, string.Empty, false, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Tests reading stream as a bytes array.
        /// </summary>
        [TestMethod]
        public void ReadStreamAsBytes()
        {
            byte[] originalBytes = new byte[] { 1, 2, 3, 4, };
            HttpContent content = CreateStreamContent(originalBytes);
            List<byte> readBytes = new List<byte>(content.ReadAsByteArrayAsync().AsTask().Result);

            TestHelper.Equal(originalBytes, readBytes);

            // Reading again: there should be nothing left in the stream.
            readBytes = new List<byte>(content.ReadAsByteArrayAsync().AsTask().Result);
            Assert.AreEqual(readBytes.Count, 0);
        }

        /// <summary>
        /// Tests buffering stream content.
        /// </summary>
        [TestMethod]
        public void BufferStreamContent()
        {
            byte[] originalBytes = new byte[] { 1, 2, 3, 4, };
            HttpContent content = CreateStreamContent(originalBytes);

            // Reading multiple times should work after buffering.
            content.CopyToBufferAsync().AsTask().Wait();

            for (int i = 0; i < 2; i++)
            {
                List<byte> readBytes = new List<byte>(
                    content.ReadAsByteArrayAsync().AsTask().Result);
                TestHelper.Equal(originalBytes, readBytes);
            }
        }

        /// <summary>
        /// Tests copying stream content into a stream.
        /// </summary>
        [TestMethod]
        public void CopyStreamContent()
        {
            byte[] originalContent = new byte[] { 1, 2, 3, 4, };
            HttpContent content = CreateStreamContent(originalContent);

            using (MemoryStream stream = new MemoryStream())
            {
                content.CopyToAsync(stream.AsOutputStream()).AsTask().Wait();
                stream.Flush();
                byte[] readContent = stream.ToArray();
                TestHelper.Equal(originalContent, readContent);
            }

            // Reading for the second time should 
            using (MemoryStream stream = new MemoryStream())
            {
                content.CopyToAsync(stream.AsOutputStream()).AsTask().Wait();
                stream.Flush();
                byte[] readContent = stream.ToArray();
                Assert.AreEqual(readContent.Length, 0);
            }
        }
    }
}
