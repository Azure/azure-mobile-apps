// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Linq;
using TestUtilities;
using Xunit;

namespace System.Net.Http.Headers
{
    public class LinkHeaderTests
    {
        private readonly Uri address = new Uri("http://localhost");
        private LinkHeaderValue header;

        public LinkHeaderTests()
        {
            this.header = new LinkHeaderValue(this.address);
        }

        public static TheoryDataCollection<string, string> Rfc5987Data
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    { "stylesheet-ä", "utf-8''stylesheet-%C3%A4" },
                    { "string", "utf-8''string" },
                    { "你好世界", "utf-8''%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C" },
                    { "ÆØÅ æøå", "utf-8''%C3%86%C3%98%C3%85%20%C3%A6%C3%B8%C3%A5" },
                };
            }
        }

        public static TheoryDataCollection<dynamic, string> HeaderValues
        {
            get
            {
                return new TheoryDataCollection<dynamic, string>
                {
                    { new { Address = "http://www.example.com#fragment", Rel = string.Empty, Rev = string.Empty, Title = string.Empty, TitleStar = string.Empty }, "http://www.example.com/#fragment" },
                    { new { Address = "http://www.example.com/path?query", Rel = "relA", Rev = "revA", Title = "titleA", TitleStar = "titleStarA" }, "http://www.example.com/path?query; rel=relA; rev=revA; title=titleA; title*=utf-8''titleStarA" },
                };
            }
        }

        [Fact]
        public void Address_Roundtrips()
        {
            Uri roundtrips = new Uri("http://www.example.com");
            PropertyAssert.Roundtrips(this.header, h => h.Address, PropertySetter.NullThrows, defaultValue: this.address, roundtripValue: roundtrips);
        }

        [Fact]
        public void Rel_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.header, h => h.Rel, PropertySetter.NullRoundtrips, roundtripValue: "rel");
        }

        [Fact]
        public void Rev_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.header, h => h.Rev, PropertySetter.NullRoundtrips, roundtripValue: "rev");
        }

        [Fact]
        public void Title_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.header, h => h.Title, PropertySetter.NullRoundtrips, roundtripValue: "title");
        }

        [Fact]
        public void TitleStar_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.header, h => h.TitleStar, PropertySetter.NullRoundtrips, roundtripValue: "titlestar");
        }

        [Theory]
        [MemberData("Rfc5987Data")]
        public void TitleStar_IsRfc5987Encoded(string input, string expected)
        {
            this.header.TitleStar = input;
            NameValueHeaderValue actual = this.header.Parameters.FirstOrDefault(item => item.Name == "title*");
            Assert.Equal(expected, actual.Value);
        }

        [Theory]
        [MemberData("HeaderValues")]
        public void ToString_WritesHeaderValue(dynamic input, string expected)
        {
            LinkHeaderValue link = new LinkHeaderValue(input.Address);
            link.Rel = input.Rel;
            link.Rev = input.Rev;
            link.Title = input.Title;
            link.TitleStar = input.TitleStar;

            string actual = link.ToString();
            Assert.Equal(expected, actual);
        }
    }
}
