//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Mobile.Server
{
    using System;
    using System.IdentityModel.Tokens;

    internal class HmacSigningCredentials : SigningCredentials
    {
        public HmacSigningCredentials(string base64EncodedKey)
            : this(ParseKeyString(base64EncodedKey))
        {
        }

        public HmacSigningCredentials(byte[] key)
            : base(new InMemorySymmetricSecurityKey(key), CreateSignatureAlgorithm(key), CreateDigestAlgorithm(key))
        {
        }

        protected static string CreateSignatureAlgorithm(byte[] key)
        {
            if (key.Length <= 32)
            {
                return Algorithms.HmacSha256Signature;
            }
            else if (key.Length <= 48)
            {
                return Algorithms.HmacSha384Signature;
            }
            else
            {
                return Algorithms.HmacSha512Signature;
            }
        }

        protected static string CreateDigestAlgorithm(byte[] key)
        {
            if (key.Length <= 32)
            {
                return Algorithms.Sha256Digest;
            }
            else if (key.Length <= 48)
            {
                return Algorithms.Sha384Digest;
            }
            else
            {
                return Algorithms.Sha512Digest;
            }
        }

        internal static class Algorithms
        {
            public const string HmacSha256Signature = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
            public const string HmacSha384Signature = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha384";
            public const string HmacSha512Signature = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha512";

            public const string Sha256Digest = "http://www.w3.org/2001/04/xmlenc#sha256";
            public const string Sha384Digest = "http://www.w3.org/2001/04/xmlenc#sha384";
            public const string Sha512Digest = "http://www.w3.org/2001/04/xmlenc#sha512";
        }

        /// <summary>
        /// Converts a base64 OR hex-encoded string into a byte array.
        /// </summary>
        internal static byte[] ParseKeyString(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                return new byte[0];
            }
            else if (IsHexString(keyString))
            {
                return HexStringToByteArray(keyString);
            }
            else
            {
                return Convert.FromBase64String(keyString);
            }
        }

        internal static bool IsHexString(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                bool isHexChar = (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
                if (!isHexChar)
                {
                    return false;
                }
            }

            return true;
        }

        internal static byte[] HexStringToByteArray(string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}
