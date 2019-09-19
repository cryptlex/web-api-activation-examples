using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cryptlex.Models
{
    public class ActivationPayload
    {
        [JsonProperty("aid")]
        public string Id { get; set; }
        public string Os { get; set; }

        [JsonProperty("fp")]
        public string Fingerprint { get; set; }

        [JsonProperty("fms")]
        public string FingerprintMatchingStrategy { get; set; }

        [JsonProperty("eat")]
        public long ExpiresAt { get; set; }

        [JsonProperty("iat")]
        public long IssuedAt { get; set; }

        [JsonProperty("pid")]
        public string ProductId { get; set; }

        [JsonProperty("orv")]
        public long OfflineResponseValidity { get; set; }

        [JsonProperty("lid")]
        public string LicenseId { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string Type { get; set; }
        public bool Suspended { get; set; }

        [JsonProperty("fc")]
        public long FloatingClients { get; set; }

        [JsonProperty("ls")]
        public string LeasingStrategy { get; set; }

        [JsonProperty("ld")]
        public long LeaseDuration { get; set; }

        [JsonProperty("ssgpeat")]
        public long ServerSyncGracePeriodExpiresAt { get; set; }

        [JsonProperty("leat")]
        public long LeaseExpiresAt { get; set; }

        [JsonProperty("aco")]
        public long AllowedClockOffset { get; set; }

        [JsonProperty("ssi")]
        public long ServerSyncInterval { get; set; }
        public bool IsValid { get; set; }

        [JsonProperty("pmd")]
        public List<Metadata> ProductMetadata { get; set; }

        [JsonProperty("lmd")]
        public List<Metadata> LicenseMetadata { get; set; }

        [JsonProperty("umd")]
        public List<Metadata> UserMetadata { get; set; }

        [JsonProperty("amd")]
        public List<Metadata> ActivationMetadata { get; set; }

        [JsonProperty("lma")]
        public List<LicenseMeterAttribute> LicenseMeterAttributes { get; set; }

        [JsonProperty("ama")]
        public List<ActivationMeterAttribute> ActivationMeterAttributes { get; set; }

        public void CopyProperties(ActivationPayload source)
        {
            // Iterate the Properties of the destination instance and  
            // populate them from their source counterparts  
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                PropertyInfo sourcePi = source.GetType().GetProperty(property.Name);
                property.SetValue(this, sourcePi.GetValue(source, null), null);
            }
        }
    }
}