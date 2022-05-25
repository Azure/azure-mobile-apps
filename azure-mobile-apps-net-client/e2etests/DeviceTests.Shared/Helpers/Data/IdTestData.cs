// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Shared.Helpers.Models;
using System;
using System.Linq;

namespace DeviceTests.Shared.Helpers.Data
{
    public static class IdTestData
    {
        private static string DisallowedCharacters = "\"#$%&'*+,/:;<=>?@\\`";

        public static string[] ValidStringIds = new string[] {
            "id",
            "true",
            "false",
            Guid.Empty.ToString(),
            "aa4da0b5-308c-4877-a5d2-03f274632636",
            "69C8BE62-A09F-4638-9A9C-6B448E9ED4E7",
            "{EC26F57E-1E65-4A90-B949-0661159D0546}",
            "87D5B05C93614F8EBFADF7BC10F7AE8C",
            "someone@someplace.com",
            "id with spaces",
            "id.with.periods",
            "'id' with single quotes",
            "id with 127 characters " + new string('x', 127 - "id with 127 characters ".Length),
            "id with Japanese 私の車はどこですか？",
            "id with Arabic أين هو سيارتي؟",
            "id with Russian Где моя машина",
            "id with allowed ascii chars " + Enumerable.Range(32, 126 - 32)
                                                            .Where( n => !DisallowedCharacters.Contains(((char)n).ToString()))
                                                            .Select( number => char.ToString((char)number))
                                                            .Aggregate("", (s, c)=> s + c),
            "id with extended ascii chars " + Enumerable.Range(160, 255 - 160)
                                                                     .Select( number => char.ToString((char)number))
                                                                     .Aggregate("", (s, c)=> s + c),
        };

        public static string[] EmptyStringIds = new string[] {
            "",
        };

        public static string[] InvalidStringIds = (new string[] {
            ".",
            "..",
            "id with 256 characters " + Enumerable.Repeat<string>("x", 256 - "id with 256 characters ".Length).Aggregate("", (s, c)=> s + c),
            "\r",
            "\n",
            "\t",
            "id\twith\ttabs",
            "id\rwith\rreturns",
            "id\nwith\n\newline",
            "id with fowardslash \\",
            "id with backslash /",
            new DateTime(2010, 1, 8).ToUniversalTime().ToString(),
            "\"idWithQuotes\"",
            "?",
            "\\",
            "/",
            "`",
            "+",
        }).Concat(Enumerable.Range(1, 32).Select(number => char.ToString((char)number)))
          .Concat(Enumerable.Range(127, 159 - 127).Select(number => char.ToString((char)number)))
          .ToArray();

        public static long[] ValidIntIds = new long[] {
            1,
            int.MaxValue,
            long.MaxValue
        };

        public static long[] InvalidIntIds = new long[] {
            -1,
            int.MinValue,
            long.MinValue,
        };

        public static object[] NonStringNonIntValidJsonIds = new object[] {
            true,
            false,
            1.0,
            -1.0,
            0.0,
        };

        public static object[] NonStringNonIntIds = new object[] {
            new PocoType(),
            new DateTime(2010, 1, 8),
            new object(),
            1.0,
            Guid.Parse("aa4da0b5-308c-4877-a5d2-03f274632636")
        };
    }
}
