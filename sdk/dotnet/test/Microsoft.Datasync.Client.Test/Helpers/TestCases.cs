// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.TestCases
{
    [ExcludeFromCodeCoverage]
    public class Invalid_Endpoints : TheoryData<string, bool>
    {
        //public Invalid_Endpoints()
        //{
        //    Add("", false);
        //    Add("", true);
        //    Add("http://", false);
        //    Add("http://", true);
        //    Add("file://localhost/foo", false);
        //    Add("http://foo.azurewebsites.net", false);
        //    Add("http://foo.azure-api.net", false);
        //    Add("http://[2001:db8:0:b:0:0:0:1A]", false);
        //    Add("http://[2001:db8:0:b:0:0:0:1A]:3000", false);
        //    Add("http://[2001:db8:0:b:0:0:0:1A]:3000/myapi", false);
        //    Add("http://10.0.0.8", false);
        //    Add("http://10.0.0.8:3000", false);
        //    Add("http://10.0.0.8:3000/myapi", false);
        //    Add("foo/bar", true);
        //}
    }

    //[ExcludeFromCodeCoverage]
    //public class Valid_Endpoints : TheoryData<string, string>
    //{
    //    public Valid_Endpoints()
    //    {
    //        string[] protocols = { "http", "https" };
    //        string[] hostnames = { "localhost", "127.0.0.1", "[::1]" };
    //        string[] ports = { "", ":3000" };
    //        string[] paths = { "", "/myapi" };

    //        foreach (var protocol in protocols)
    //        {
    //            foreach (var hostname in hostnames)
    //            {
    //                foreach (var port in ports)
    //                {
    //                    foreach (var path in paths)
    //                    {
    //                        var uri = $"{protocol}://{hostname}{port}{path}";
    //                        Add(uri, uri + "/");
    //                        Add(uri + "?queryparam", uri + "/");
    //                        Add(uri + "#fragment", uri + "/");
    //                        Add(uri + "/?queryparam#fragment", uri + "/");
    //                    }
    //                }
    //            }
    //        }

    //        string[] securehosts = { "myapi.azurewebsites.net", "myapi.azure-api.net" };
    //        foreach (var hostname in securehosts)
    //        {
    //            foreach (var port in ports)
    //            {
    //                foreach (var path in paths)
    //                {
    //                    var uri = $"https://{hostname}{port}{path}";
    //                    Add(uri, uri + "/");
    //                    Add(uri + "?queryparam", uri + "/");
    //                    Add(uri + "#fragment", uri + "/");
    //                    Add(uri + "/?queryparam#fragment", uri + "/");
    //                }
    //            }
    //        }
    //    }
    //}
}
