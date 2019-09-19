using System;
using Newtonsoft.Json;

using Cryptlex.Models;

namespace Cryptlex.Services
{
    public class LexValidator
    {
        public static int ValidateActivation(string jwt, string publicKey, string licenseKey, string productId, ActivationPayload activationPayload)
        {
            string payload = LexJwtService.VerifyToken(jwt, publicKey);
            if (String.IsNullOrEmpty(payload))
            {
                return LexStatusCodes.LA_FAIL;
            }
            var tempActivationPayload = JsonConvert.DeserializeObject<ActivationPayload>(payload);
            activationPayload.CopyProperties(tempActivationPayload);
            activationPayload.IsValid = true;
            int status;
            if (licenseKey != activationPayload.Key)
            {
                status = LexStatusCodes.LA_FAIL;
            }
            else if (productId != activationPayload.ProductId)
            {
                status = LexStatusCodes.LA_FAIL;
            }
            else if (activationPayload.Fingerprint != LexSystemInfo.GetFingerPrint())
            {
                status = LexStatusCodes.LA_E_MACHINE_FINGERPRINT;
            }
            else if (!ValidateTime(activationPayload.IssuedAt, activationPayload.AllowedClockOffset))
            {
                status = LexStatusCodes.LA_E_TIME;
            }
            else
            {
                status = LexValidator.ValidateActivationStatus(productId, activationPayload);
            }
            if (status == LexStatusCodes.LA_OK || status == LexStatusCodes.LA_EXPIRED || status == LexStatusCodes.LA_SUSPENDED || status == LexStatusCodes.LA_GRACE_PERIOD_OVER)
            {
                var now = GetUtcTimestamp();
                LexDataStore.SaveValue(productId, LexConstants.KEY_LAST_RECORDED_TIME, now.ToString());
                LexDataStore.SaveValue(productId, LexConstants.KEY_ACTIVATION_JWT, jwt);
            }
            else
            {
                LexDataStore.SaveValue(productId, LexConstants.KEY_LAST_RECORDED_TIME, activationPayload.IssuedAt.ToString());
            }
            return status;
        }

        public static int ValidateActivationStatus(string productId, ActivationPayload activationPayload)
        {
            var now = GetUtcTimestamp();

            if (activationPayload.LeaseExpiresAt != 0 && (activationPayload.LeaseExpiresAt < now || activationPayload.LeaseExpiresAt < activationPayload.IssuedAt))
            {
                LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                return LexStatusCodes.LA_FAIL;
            }

            bool skipGracePeriodCheck = (activationPayload.ServerSyncInterval == 0 || activationPayload.ServerSyncGracePeriodExpiresAt == 0);
            if (!skipGracePeriodCheck && activationPayload.ServerSyncGracePeriodExpiresAt < now)
            {
                return LexStatusCodes.LA_GRACE_PERIOD_OVER;
            }
            else if (activationPayload.ExpiresAt != 0 && activationPayload.ExpiresAt < now)
            {
                return LexStatusCodes.LA_EXPIRED;
            }

            else if (activationPayload.ExpiresAt != 0 && activationPayload.ExpiresAt < activationPayload.IssuedAt)
            {
                return LexStatusCodes.LA_EXPIRED;
            }

            else if (activationPayload.Suspended)
            {
                return LexStatusCodes.LA_SUSPENDED;
            }
            else
            {
                return LexStatusCodes.LA_OK;
            }
        }

        public static bool ValidateProductId(string productId)
        {
            Guid guid;
            if (!Guid.TryParse(productId, out guid))
            {
                return false;
            }
            return true;
        }

        public static bool ValidateLicenseKey(string licenseKey)
        {
            if (licenseKey.Length < LexConstants.MIN_PRODUCT_KEY_LENGTH)
            {
                return false;
            }
            if (licenseKey.Length > LexConstants.MAX_PRODUCT_KEY_LENGTH)
            {
                return false;
            }
            return true;
        }

        public static bool ValidateSuccessCode(int status)
        {
            if (status == LexStatusCodes.LA_OK || status == LexStatusCodes.LA_EXPIRED || status == LexStatusCodes.LA_GRACE_PERIOD_OVER || status == LexStatusCodes.LA_SUSPENDED || status == LexStatusCodes.LA_TRIAL_EXPIRED)
            {
                return true;
            }
            return false;
        }

        public static bool ValidateServerSyncAllowedStatusCodes(long status)
        {
            if (status == LexStatusCodes.LA_OK || status == LexStatusCodes.LA_EXPIRED || status == LexStatusCodes.LA_SUSPENDED)
            {
                return true;
            }
            if (status == LexStatusCodes.LA_E_INET || status == LexStatusCodes.LA_E_RATE_LIMIT || status == LexStatusCodes.LA_E_SERVER)
            {
                return true;
            }
            return false;

        }

        public static bool ValidateTime(long timestamp, long allowedClockOffset)
        {
            var now = GetUtcTimestamp();
            long timeDifference = (long)(timestamp - now);
            if (timeDifference > allowedClockOffset)
            {
                return false;
            }
            return true;
        }

        public static bool ValidateSystemTime(string productId)
        {
            var now = GetUtcTimestamp();
            var lastRecordedTimeStr = LexDataStore.GetValue(productId, LexConstants.KEY_LAST_RECORDED_TIME);
            if (ValidateTime((long)Int32.Parse(lastRecordedTimeStr), LexConstants.ALLOWED_CLOCK_OFFSET))
            {
                LexDataStore.SaveValue(productId, LexConstants.KEY_LAST_RECORDED_TIME, now.ToString());
                return true;
            }
            return false;
        }

        public static long GetUtcTimestamp()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}