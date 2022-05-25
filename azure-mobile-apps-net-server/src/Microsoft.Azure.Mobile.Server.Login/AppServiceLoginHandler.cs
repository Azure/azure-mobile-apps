// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile.Server.Login
{
    public static class AppServiceLoginHandler
    {
        public static JwtSecurityToken CreateToken(IEnumerable<Claim> claims, string secretKey, string audience, string issuer, TimeSpan? lifetime)
        {
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }

            if (lifetime != null && lifetime < TimeSpan.Zero)
            {
                string msg = CommonResources.ArgMustBeGreaterThanOrEqualTo.FormatForUser(TimeSpan.Zero);
                throw new ArgumentOutOfRangeException("lifetime", lifetime, msg);
            }

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException("secretKey");
            }

            if (claims.SingleOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub) == null)
            {
                throw new ArgumentOutOfRangeException("claims", LoginResources.CreateToken_SubjectRequired);
            }

            // add the claims passed in
            Collection<Claim> finalClaims = new Collection<Claim>();
            foreach (Claim claim in claims)
            {
                finalClaims.Add(claim);
            }

            // add our standard claims
            finalClaims.Add(new Claim("ver", "3"));

            return CreateTokenFromClaims(finalClaims, secretKey, audience, issuer, lifetime);
        }

        internal static JwtSecurityToken CreateTokenFromClaims(IEnumerable<Claim> claims, string secretKey, string audience, string issuer, TimeSpan? lifetime)
        {
            DateTime created = DateTime.UtcNow;

            // we allow for no expiry (if lifetime is null)
            DateTime? expiry = (lifetime != null) ? created + lifetime : null;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                AppliesToAddress = audience,
                TokenIssuerName = issuer,
                SigningCredentials = new HmacSigningCredentials(secretKey),
                Lifetime = new Lifetime(created, expiry),
                Subject = new ClaimsIdentity(claims),
            };

            var securityTokenHandler = new JwtSecurityTokenHandler() { SetDefaultTimesOnTokenCreation = false };            
            return securityTokenHandler.CreateToken(tokenDescriptor) as JwtSecurityToken;
        }
    }
}