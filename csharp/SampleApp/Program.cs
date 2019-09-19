using System;
using System.Threading.Tasks;
using Cryptlex;

namespace Sample
{
    class Program
    {
        public static LicenseManager licenseManager = new LicenseManager();
        static void Init()
        {
            licenseManager.SetProductId("bb65d1d9-34a9-4add-9f73-61fc49fc91ed");
            licenseManager.SetRsaPublicKey("C:/Users/Administrator/Desktop/test/sample-app/rsa_pub.pem");
            licenseManager.SetAppVersion("1.0.0");
        }

        static void Activate()
        {
            licenseManager.SetLicenseKey("B232CA-69665B-4617AB-7836B1-2885C7-881621");
            int status = licenseManager.ActivateLicense();
            if (LexStatusCodes.LA_OK == status || LexStatusCodes.LA_EXPIRED == status || LexStatusCodes.LA_SUSPENDED == status)
            {
                Console.WriteLine("License activated successfully: ", status);
            }
            else
            {
                Console.WriteLine("License activation failed: ", status);
            }
        }
        static void Main(string[] args)
        {
            try
            {
                Init();
                Activate();
                licenseManager.SetLicenseCallback(LicenseCallback);
                int status = licenseManager.IsLicenseGenuine();
                if (LexStatusCodes.LA_OK == status)
                {
                    Console.WriteLine("License is genuinely activated!");
                    long expiryDate = licenseManager.GetLicenseExpiryDate();
                    long daysLeft = (expiryDate - GetUtcTimestamp()) / 86400;
                    Console.WriteLine("Days left:" + daysLeft);
                }
                else if (LexStatusCodes.LA_EXPIRED == status)
                {
                    Console.WriteLine("License is genuinely activated but has expired!");
                }
                else if (LexStatusCodes.LA_GRACE_PERIOD_OVER == status)
                {
                    Console.WriteLine("License is genuinely activated but grace period is over!");
                }
                else if (LexStatusCodes.LA_SUSPENDED == status)
                {
                    Console.WriteLine("License is genuinely activated but has been suspended!");
                }
                else
                {
                    Console.WriteLine("License is not activated!");
                }

                //  licenseManager.IncrementActivationMeterAttributeUses("channels", 10);
                //  licenseManager.DecrementActivationMeterAttributeUses("wheels", 10);
                //  licenseManager.DecrementActivationMeterAttributeUses("channels", 10);
                //  licenseManager.ResetActivationMeterAttributeUses("channels");
                licenseManager.DeactivateLicense();

            }
            catch (LexActivatorException ex)
            {
                Console.WriteLine("Error code: " + ex.Code.ToString() + " Error message: " + ex.Message);
            }
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }

        // License callback is invoked when LexActivator.IsLicenseGenuine() completes a server sync
        static void LicenseCallback(int status)
        {
            switch (status)
            {
                case LexStatusCodes.LA_SUSPENDED:
                    Console.WriteLine("The license has been suspended.");
                    break;
                default:
                    Console.WriteLine("License status code: " + status.ToString());
                    break;
            }
        }

        public static long GetUtcTimestamp()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}