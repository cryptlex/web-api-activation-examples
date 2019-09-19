using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using Newtonsoft.Json;

using Cryptlex.Models;

namespace Cryptlex
{
    public class LexJwtService
    {

        public static string VerifyToken(string jwt, string publicKey)
        {
            try
            {
                var securityKey = LexEncryptionService.PublicKeyFromPem(publicKey);
                var validationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = false,
                    RequireExpirationTime = false,
                    IssuerSigningKey = securityKey
                };
                SecurityToken validatedToken;
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                var principalClaim = handler.ValidateToken(jwt, validationParameters, out validatedToken);
                var payload = handler.ReadJwtToken(jwt).Payload;
                payload["pmd"] = JsonConvert.DeserializeObject<List<Metadata>>(payload["pmd"].ToString());
                payload["lmd"] = JsonConvert.DeserializeObject<List<Metadata>>(payload["lmd"].ToString());
                payload["umd"] = JsonConvert.DeserializeObject<List<Metadata>>(payload["umd"].ToString());
                payload["amd"] = JsonConvert.DeserializeObject<List<Metadata>>(payload["amd"].ToString());
                payload["lma"] = JsonConvert.DeserializeObject<List<LicenseMeterAttribute>>(payload["lma"].ToString());
                payload["ama"] = JsonConvert.DeserializeObject<List<ActivationMeterAttribute>>(payload["ama"].ToString());
                var payloadJson = JsonConvert.SerializeObject(payload);
                return payloadJson;
            }
            catch
            {
                return null;
            }
        }
    }
}