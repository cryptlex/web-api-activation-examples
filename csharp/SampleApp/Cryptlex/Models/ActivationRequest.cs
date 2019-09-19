using System.Collections.Generic;

namespace Cryptlex.Models
{
    public class ActivationRequest
    {   
        /// <summary>
        /// License key to activate the license.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Name of the operating system.
        /// </summary>

        public string Os { get; set; }

        /// <summary>
        /// Version of the operating system.
        /// </summary>
        public string OsVersion { get; set; }

        /// <summary>
        /// Fingerprint of the machine.
        /// </summary>
        public string Fingerprint { get; set; }

        /// <summary>
        /// Name of the virtual machine.
        /// </summary>
        public string VmName { get; set; }

        /// <summary>
        /// Name of the host machine.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Version of the application.
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// Version of the client.
        /// </summary>
        public string ClientVersion { get; set; }

        /// <summary>
        /// Hash of the machine user name.
        /// </summary>
        public string UserHash { get; set; }

        /// <summary>
        /// Unique identifier for the product.
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password of the user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// List of metdata key/value pairs.
        /// </summary>
        public List<ActivationMetadata> Metadata { get; set; }

        /// <summary>
        /// List of meter attributes.
        /// </summary>
        public List<ActivationMeterAttribute> MeterAttributes { get; set; }

    }

    public class ActivationMeterAttribute
    {
        public ActivationMeterAttribute(string name, long uses)
        {
            Name = name;
            Uses = uses;
        }
        public string Name { get; set; }

        public long Uses { get; set; }
    }

    public class ActivationMetadata
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
