// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Shared.Helpers.Data;
using DeviceTests.Shared.Helpers.Models;
using DeviceTests.Shared.TestPlatform;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DeviceTests.Shared.Tests
{
    [Collection(nameof(SingleThreadedCollection))]
    public class CustomAPI_Tests : E2ETestBase
    {
        private const string AppApiName = "applicationPermission";

        public enum ApiPermissions { Public, Application, User, Admin }

        public enum TypedTestType { GetByTitle, GetByDate, PostByDuration, PostByYear }

        public enum DataFormat { Json, Xml, Other }

        private const string Letters = "abcdefghijklmnopqrstuvwxyz";
        private const string MovieFinderApiName = "movieFinder";

        // this test is being removed due to failures on windows phone, and the fact it
        // doesn't actually exercise the SDK at all - XML handling is entirely custom API code
        //[Fact]
        //private async Task CustomAPI_JToken_SupportsAnyFormat()
        //{
        //    DateTime now = DateTime.UtcNow;
        //    int seed = now.Year * 10000 + now.Month * 100 + now.Day;
        //    Random rndGen = new Random(seed);

        //    // Log the seed for future repros.
        //    Log("Seed: {0}", seed);

        //    IMobileServiceClient client = GetClient();

        //    foreach (DataFormat inputFormat in Enum.GetValues(typeof(DataFormat)))
        //    {
        //        foreach (DataFormat outputFormat in Enum.GetValues(typeof(DataFormat)))
        //        {
        //            await customApiDataTest(inputFormat, outputFormat, rndGen);
        //        }
        //    }
        //}

        [Fact]
        private async Task CustomAPI_Typed_SupportsJson()
        {
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Random rndGen = new Random(seed);

            foreach (TypedTestType testType in Enum.GetValues(typeof(TypedTestType)))
            {
                await CreateTypedApiTest(rndGen, testType);
            }
        }

        private async Task CreateTypedApiTest(Random seedGenerator, TypedTestType testType)
        {
            var client = GetClient();
            var apiName = MovieFinderApiName;
            for (int i = 0; i < 10; i++)
            {
                int seed = seedGenerator.Next();
                Random rndGen = new Random(seed);

                Movie[] expectedResult = null;
                AllMovies actualResult = null;
                Movie inputTemplate;

                // TODO: BUG #2132434: .NET runtime should allow URI's that end with a dot
                while (true)
                {
                    inputTemplate = QueryTestData.TestMovies()[rndGen.Next(QueryTestData.TestMovies().Length)];
                    if (testType == TypedTestType.GetByTitle && inputTemplate.Title.EndsWith("."))
                    {
                        // The .NET backend barfs and returns 404 if the URI ends with a dot, so let's get another movie
                        continue;
                    }

                    if (testType == TypedTestType.GetByTitle && inputTemplate.Title.EndsWith("?"))
                    {
                        // The .NET & Node backend do not return anything if the URI ends with a '?' so let's get another movie
                        continue;
                    }
                    break;
                }

                string apiUrl;
                switch (testType)
                {
                    case TypedTestType.GetByTitle:
                        apiUrl = apiName + "/title/" + inputTemplate.Title;
                        expectedResult = new Movie[] { inputTemplate };
                        actualResult = await client.InvokeApiAsync<AllMovies>(apiUrl, HttpMethod.Get, null);
                        break;

                    case TypedTestType.GetByDate:
                        var releaseDate = inputTemplate.ReleaseDate;
                        apiUrl = apiName + "/date/" + releaseDate.Year + "/" + releaseDate.Month + "/" + releaseDate.Day;
                        expectedResult = QueryTestData.TestMovies().Where(m => m.ReleaseDate == releaseDate).ToArray();
                        actualResult = await client.InvokeApiAsync<AllMovies>(apiUrl, HttpMethod.Get, null);
                        break;

                    case TypedTestType.PostByDuration:
                    case TypedTestType.PostByYear:
                        string orderBy = null;
                        switch (rndGen.Next(3))
                        {
                            case 0:
                                orderBy = null;
                                break;

                            case 1:
                                orderBy = "id";
                                break;

                            case 2:
                                orderBy = "Title";
                                break;
                        }

                        Dictionary<string, string> queryParams = orderBy == null ?
                            null :
                            new Dictionary<string, string> { { "orderBy", orderBy } };

                        Func<Movie, bool> predicate;
                        if (testType == TypedTestType.PostByYear)
                        {
                            predicate = m => m.Year == inputTemplate.Year;
                            apiUrl = apiName + "/moviesOnSameYear";
                        }
                        else
                        {
                            predicate = m => m.Duration == inputTemplate.Duration;
                            apiUrl = apiName + "/moviesWithSameDuration";
                        }

                        if (queryParams == null)
                        {
                            actualResult = await client.InvokeApiAsync<Movie, AllMovies>(apiUrl, inputTemplate);
                        }
                        else
                        {
                            actualResult = await client.InvokeApiAsync<Movie, AllMovies>(apiUrl, inputTemplate, HttpMethod.Post, queryParams);
                        }

                        expectedResult = QueryTestData.TestMovies().Where(predicate).ToArray();
                        if (orderBy == null || orderBy == "Title")
                        {
                            Array.Sort(expectedResult, (m1, m2) => m1.Title.CompareTo(m2.Title));
                        }

                        break;

                    default:
                        throw new ArgumentException("Invalid test type: " + testType);
                }
                List<string> errors = new List<string>();
                var actual = actualResult.Movies;
                Assert.True(!expectedResult.Except(actual).Any() && expectedResult.Length == actual.Length);
            }
        }

        private async Task customApiDataTest(DataFormat inputFormat, DataFormat outputFormat, Random seedGenerator)
        {
            for (int i = 0; i < 10; i++)
            {
                int seed = seedGenerator.Next();
                Random rndGen = new Random(seed);

                JToken body = CreateJson(rndGen);

                Dictionary<string, string> headers = new Dictionary<string, string>() { { "aa", "" } };
                CreateHttpContentTestInput(inputFormat, outputFormat, rndGen, out HttpMethod method, out HttpContent content, 
                    out JObject expectedResult, out headers, out Dictionary<string, string> query, out HttpStatusCode expectedStatus);

                using (HttpResponseMessage response = await InvokeApiAsync(AppApiName, content, method, headers, query))
                {
                    ValidateResponseHeader(expectedStatus, headers, response);
                    string responseContent = null;
                    if (response.Content != null)
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                    }

                    JToken jsonResponse = null;
                    if (outputFormat == DataFormat.Json)
                    {
                        jsonResponse = JToken.Parse(responseContent);
                    }
                    else if (outputFormat == DataFormat.Other)
                    {
                        string decodedContent = responseContent
                            .Replace("__{__", "{")
                            .Replace("__}__", "}")
                            .Replace("__[__", "[")
                            .Replace("__]__", "]");
                        jsonResponse = JToken.Parse(decodedContent);
                    }

                    bool contentIsExpected = false;
                    List<string> errors = new List<string>();
                    switch (outputFormat)
                    {
                        case DataFormat.Json:
                        case DataFormat.Other:
                            contentIsExpected = CompareJson(expectedResult, jsonResponse, errors);
                            break;

                        case DataFormat.Xml:
                            string expectedResultContent = JsonToXml(expectedResult);

                            // Normalize CRLF
                            expectedResultContent = expectedResultContent.Replace("\r\n", "\n");
                            responseContent = responseContent.Replace("\r\n", "\n");

                            Assert.Equal(expectedResultContent, responseContent);
                            break;
                    }
                }
            }
        }

        private async Task<HttpResponseMessage> InvokeApiAsync(string apiName, HttpContent content, HttpMethod method, Dictionary<string, string> headers, Dictionary<string, string> query)
        {
            HttpResponseMessage response = null;

            try
            {
                response = await GetClient().InvokeApiAsync(apiName, content, method, headers, query);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                response = e.Response;
            }

            return response;
        }

        private static void CreateHttpContentTestInput(DataFormat inputFormat, DataFormat outputFormat, Random rndGen, out HttpMethod method, out HttpContent content, out JObject expectedResult, out Dictionary<string, string> headers, out Dictionary<string, string> query, out HttpStatusCode expectedStatus)
        {
            method = CreateHttpMethod(rndGen);
            content = null;
            expectedResult = new JObject();
            expectedResult.Add("method", method.Method);
            expectedResult.Add("user", new JObject(new JProperty("level", "anonymous")));
            JToken body = null;
            string textBody = null;
            if (method.Method != "GET" && method.Method != "DELETE")
            {
                body = CreateJson(rndGen);
                if (outputFormat == DataFormat.Xml || inputFormat == DataFormat.Xml)
                {
                    // to prevent non-XML names from interfering with checks
                    body = SanitizeJsonXml(body);
                }

                switch (inputFormat)
                {
                    case DataFormat.Json:
                        // JSON
                        content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
                        break;

                    case DataFormat.Xml:
                        textBody = JsonToXml(body);
                        content = new StringContent(textBody, Encoding.UTF8, "text/xml");
                        break;

                    default:
                        textBody = body.ToString().Replace("{", "<").Replace("}", ">").Replace("[", "__[__").Replace("]", "__]__");
                        content = new StringContent(textBody, Encoding.UTF8, "text/plain");
                        break;
                }
            }

            if (body != null)
            {
                if (inputFormat == DataFormat.Json)
                {
                    expectedResult.Add("body", body);
                }
                else
                {
                    expectedResult.Add("body", textBody);
                }
            }

            headers = new Dictionary<string, string>();
            var choice = rndGen.Next(5);
            for (int j = 0; j < choice; j++)
            {
                string name = "x-test-zumo-" + j;
                string value = CreateString(rndGen, 1, 10, Letters);
                headers.Add(name, value);
            }

            query = CreateQueryParams(rndGen) ?? new Dictionary<string, string>();
            if (query.Count > 0)
            {
                JObject outputQuery = new JObject();
                expectedResult.Add("query", outputQuery);
                foreach (var kvp in query)
                {
                    outputQuery.Add(kvp.Key, kvp.Value);
                }
            }

            query.Add("format", outputFormat.ToString().ToLowerInvariant());
            expectedStatus = HttpStatusCode.OK;
            if (rndGen.Next(4) == 0)
            {
                // non-200 responses
                int[] options = new[] { 400, 404, 500, 201 };
                int status = options[rndGen.Next(options.Length)];
                expectedStatus = (HttpStatusCode)status;
                query.Add("status", status.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static HttpMethod CreateHttpMethod(Random rndGen)
        {
            switch (rndGen.Next(10))
            {
                case 0:
                case 1:
                case 2:
                    return HttpMethod.Post;

                case 3:
                case 4:
                case 5:
                case 6:
                    return HttpMethod.Get;

                case 7:
                    return HttpMethod.Put;

                case 8:
                    return HttpMethod.Delete;

                default:
                    return new HttpMethod("PATCH");
            }
        }

        private static JToken SanitizeJsonXml(JToken body)
        {
            switch (body.Type)
            {
                case JTokenType.Null:
                    return JToken.Parse("null");

                case JTokenType.Boolean:
                case JTokenType.String:
                case JTokenType.Integer:
                case JTokenType.Float:
                    return new JValue((JValue)body);

                case JTokenType.Array:
                    JArray array = (JArray)body;
                    return new JArray(array.Select(jt => SanitizeJsonXml(jt)));

                case JTokenType.Object:
                    JObject obj = (JObject)body;
                    return new JObject(
                        obj.Properties().Select((jp, i) =>
                            new JProperty("member" + i, SanitizeJsonXml(jp.Value))));

                default:
                    throw new ArgumentException("Invalid type: " + body.Type);
            }
        }

        private static string JsonToXml(JToken json)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<root>");
            JsonToXml(json, sb);
            sb.Append("</root>");
            return sb.ToString();
        }

        private static void JsonToXml(JToken json, StringBuilder sb)
        {
            if (json == null)
            {
                json = "";
            }

            switch (json.Type)
            {
                case JTokenType.Null:
                    sb.Append("null");
                    break;

                case JTokenType.Boolean:
                    sb.Append(json.ToString().ToLowerInvariant());
                    break;

                case JTokenType.Float:
                case JTokenType.Integer:
                    sb.Append(json.ToString());
                    break;

                case JTokenType.String:
                    sb.Append(json.ToObject<string>());
                    break;

                case JTokenType.Array:
                    sb.Append("<array>");
                    JArray array = (JArray)json;
                    for (int i = 0; i < array.Count; i++)
                    {
                        sb.Append("<item>");
                        JsonToXml(array[i], sb);
                        sb.Append("</item>");
                    }

                    sb.Append("</array>");
                    break;

                case JTokenType.Object:
                    JObject obj = (JObject)json;
                    var keys = obj.Properties().Select(p => p.Name).ToArray();
                    Array.Sort(keys);
                    foreach (var key in keys)
                    {
                        sb.Append("<" + key + ">");
                        JsonToXml(obj[key], sb);
                        sb.Append("</" + key + ">");
                    }

                    break;

                default:
                    throw new ArgumentException("Type " + json.Type + " is not supported");
            }
        }

        private static JToken CreateJson(Random rndGen, int currentDepth = 0, bool canBeNull = true)
        {
            const int maxDepth = 3;
            int kind = rndGen.Next(15);

            // temp workaround
            if (currentDepth == 0)
            {
                kind = rndGen.Next(8, 15);
            }

            switch (kind)
            {
                case 0:
                    return true;

                case 1:
                    return false;

                case 2:
                    return rndGen.Next();

                case 3:
                    return rndGen.Next() >> rndGen.Next(10);

                case 4:
                case 5:
                case 6:
                    return CreateString(rndGen, 0, 10);

                case 7:
                    if (canBeNull)
                    {
                        return JToken.Parse("null");
                    }
                    else
                    {
                        return CreateString(rndGen, 0, 10);
                    }
                case 8:
                case 9:
                case 10:
                    if (currentDepth > maxDepth)
                    {
                        return "max depth";
                    }
                    else
                    {
                        int size = rndGen.Next(5);
                        JArray result = new JArray();
                        for (int i = 0; i < size; i++)
                        {
                            result.Add(CreateJson(rndGen, currentDepth + 1));
                        }

                        return result;
                    }
                default:
                    if (currentDepth > maxDepth)
                    {
                        return "max depth";
                    }
                    else
                    {
                        int size = rndGen.Next(5);
                        JObject result = new JObject();
                        for (int i = 0; i < size; i++)
                        {
                            string key;
                            do
                            {
                                key = CreateString(rndGen, 3, 5);
                            } while (result[key] != null);
                            result.Add(key, CreateJson(rndGen, currentDepth + 1));
                        }

                        return result;
                    }
            }
        }

        private static string CreateString(Random rndGen, int minLength = 0, int maxLength = 30, string specificChars = null)
        {
            int length = rndGen.Next(minLength, maxLength);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (specificChars == null)
                {
                    if (rndGen.Next(3) > 0)
                    {
                        // common case, ascii characters
                        sb.Append((char)rndGen.Next(' ', '~'));
                    }
                    else
                    {
                        // all unicode, except surrogate
                        char c;
                        do
                        {
                            c = (char)rndGen.Next(' ', 0xfffe);
                        } while (Char.IsSurrogate(c));
                        sb.Append(c);
                    }
                }
                else
                {
                    sb.Append(specificChars[rndGen.Next(specificChars.Length)]);
                }
            }

            return sb.ToString();
        }

        private static Dictionary<string, string> CreateQueryParams(Random rndGen)
        {
            if (rndGen.Next(2) == 0)
            {
                return null;
            }

            var result = new Dictionary<string, string>();
            var size = rndGen.Next(5);
            for (int i = 0; i < size; i++)
            {
                var name = CreateString(rndGen, 1, 10, Letters);
                var value = CreateString(rndGen);
                if (!result.ContainsKey(name))
                {
                    result.Add(name, value);
                }
            }

            return result;
        }

        private static void ValidateResponseHeader(HttpStatusCode expectedStatus, Dictionary<string, string> expectedHeaders, HttpResponseMessage response)
        {
            Assert.Equal(expectedStatus, response.StatusCode);

            foreach (var reqHeader in expectedHeaders.Keys)
            {
                Assert.True(response.Headers.TryGetValues(reqHeader, out IEnumerable<string> headerValue));
                Assert.Equal(expectedHeaders[reqHeader], headerValue.FirstOrDefault());
            }
        }

        public static bool CompareJson(JToken expected, JToken actual, List<string> errors)
        {
            if (expected == null)
            {
                return actual == null;
            }

            if (actual == null)
            {
                return false;
            }

            if (expected.Type != actual.Type)
            {
                errors.Add(string.Format("Expected value type {0} != actual {1}", expected.Type, actual.Type));
                return false;
            }

            switch (expected.Type)
            {
                case JTokenType.Boolean:
                    return expected.Value<bool>() == actual.Value<bool>();

                case JTokenType.Null:
                    return true;

                case JTokenType.String:
                case JTokenType.Date:
                    return expected.Value<string>() == actual.Value<string>();

                case JTokenType.Float:
                case JTokenType.Integer:
                    double expectedNumber = expected.Value<double>();
                    double actualNumber = actual.Value<double>();
                    double delta = 1 - expectedNumber / actualNumber;
                    double acceptableEpsilon = 0.000001;
                    if (Math.Abs(delta) > acceptableEpsilon)
                    {
                        errors.Add(string.Format("Numbers differ more than the allowed difference: {0} - {1}",
                            expected, actual));
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case JTokenType.Array:
                    JArray expectedArray = (JArray)expected;
                    JArray actualArray = (JArray)actual;
                    if (expectedArray.Count != actualArray.Count)
                    {
                        errors.Add(string.Format("Size of arrays are different: expected {0} != actual {1}", expectedArray.Count, actualArray.Count));
                        return false;
                    }

                    for (int i = 0; i < expectedArray.Count; i++)
                    {
                        if (!CompareJson(expectedArray[i], actualArray[i], errors))
                        {
                            errors.Add("Difference in array item at index " + i);
                            return false;
                        }
                    }

                    return true;

                case JTokenType.Object:
                    JObject expectedObject = (JObject)expected;
                    Dictionary<string, string> expectedKeyMap = new Dictionary<string, string>();
                    foreach (var child in expectedObject)
                    {
                        expectedKeyMap.Add(child.Key.ToLowerInvariant(), child.Key);
                    }

                    JObject actualObject = (JObject)actual;
                    Dictionary<string, string> actualKeyMap = new Dictionary<string, string>();
                    foreach (var child in actualObject)
                    {
                        actualKeyMap.Add(child.Key.ToLowerInvariant(), child.Key);
                    }

                    foreach (var child in expectedObject)
                    {
                        var key = child.Key.ToLowerInvariant();
                        if (key == "id") continue; // set by server, ignored at comparison

                        if (!actualKeyMap.ContainsKey(key) || actualObject[actualKeyMap[key]] == null)
                        {
                            // Still might be OK, if the missing value is default.
                            var expectedObjectValue = expectedObject[expectedKeyMap[key]];

                            if (expectedObjectValue.Type == JTokenType.Null ||
                                (expectedObjectValue.Type == JTokenType.Integer && expectedObjectValue.Value<long>() == 0) ||
                                (expectedObjectValue.Type == JTokenType.Float && expectedObjectValue.Value<double>() == 0.0))
                            {
                                // No problem.
                            }
                            else
                            {
                                errors.Add(string.Format("Expected object contains a pair with key {0}, actual does not.", child.Key));
                                return false;
                            }
                        }
                        else
                        {
                            if (!CompareJson(expectedObject[expectedKeyMap[key]], actualObject[actualKeyMap[key]], errors))
                            {
                                errors.Add("Difference in object member with key " + key);
                                return false;
                            }
                        }
                    }

                    return true;

                default:
                    throw new ArgumentException("Don't know how to compare JToken of type " + expected.Type);
            }
        }
    }
}
