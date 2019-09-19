using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

using System.Net;
using System.Net.Http;

using Cryptlex.Models;

namespace Cryptlex.Services
{
    public class LexActivationService
    {

        public static int ActivateFromServer(string productId, string licenseKey, string publicKey, ActivationPayload activationPayload, List<ActivationMeterAttribute> meterAttributes, bool serverSync = false)
        {
            var metadata = new List<ActivationMetadata>();
            string jsonBody = GetActivationRequest(licenseKey, productId, metadata, meterAttributes);
            var httpService = new LexHttpService();
            HttpResponseMessage httpResponse;
            try
            {
                if (serverSync)
                {
                    httpResponse = httpService.UpdateActivation(activationPayload.Id, jsonBody);
                }
                else
                {
                    httpResponse = httpService.CreateActivation(jsonBody);
                }
            }
            catch (Exception exception)
            {
                System.Console.WriteLine(exception.Message);
                return LexStatusCodes.LA_E_INET;
            }


            //             if (serverSync && LexThread.ActivationValidity.count(activationPayload.id) && LexThread.ActivationValidity[activationPayload.id] == false)
            //             {
            // # ifdef LEX_DEBUG

            //                 LexLogger.LogDebug("Ignore the response as user deactivated the key.");
            // #endif
            //                 return LexStatusCodes.LA_FAIL;
            //             }

            if (!httpResponse.IsSuccessStatusCode)
            {
                return ActivationErrorHandler(productId, httpResponse);
            }
            var json = httpResponse.Content.ReadAsStringAsync().Result;
            var activationResponse = JsonConvert.DeserializeObject<ActivationResponse>(json);
            string jwt = activationResponse.ActivationToken;
            return LexValidator.ValidateActivation(jwt, publicKey, licenseKey, productId, activationPayload);
        }

        public static int ActivationErrorHandler(string productId, HttpResponseMessage httpResponse)
        {
            if (httpResponse.StatusCode == HttpStatusCode.InternalServerError || httpResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                return LexStatusCodes.LA_E_SERVER;
            }
            if (httpResponse.StatusCode == (HttpStatusCode)LexConstants.HttpTooManyRequests)
            {
                return LexStatusCodes.LA_E_RATE_LIMIT;
            }
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                return LexStatusCodes.LA_E_ACTIVATION_NOT_FOUND;
            }
            if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorResponse = JsonConvert.DeserializeObject<HttpErrorResponse>(httpResponse.Content.ReadAsStringAsync().Result);
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.ACTIVATION_LIMIT_REACHED)
                {
                    return LexStatusCodes.LA_E_ACTIVATION_LIMIT;
                }
                // server sync fp validation failed
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.INVALID_ACTIVATION_FINGERPRINT)
                {
                    LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                    return LexStatusCodes.LA_E_MACHINE_FINGERPRINT;
                }
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.VM_ACTIVATION_NOT_ALLOWED)
                {
                    LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                    return LexStatusCodes.LA_E_VM;
                }
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.INVALID_PRODUCT_ID)
                {
                    LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                    return LexStatusCodes.LA_E_PRODUCT_ID;
                }
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.INVALID_LICENSE_KEY)
                {
                    LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                    return LexStatusCodes.LA_E_LICENSE_KEY;
                }
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.AUTHENTICATION_FAILED)
                {
                    LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                    return LexStatusCodes.LA_E_AUTHENTICATION_FAILED;
                }
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.COUNTRY_NOT_ALLOWED)
                {
                    LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                    return LexStatusCodes.LA_E_COUNTRY;
                }
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.IP_ADDRESS_NOT_ALLOWED)
                {
                    LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                    return LexStatusCodes.LA_E_IP;
                }
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.REVOKED_LICENSE)
                {
                    LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                    return LexStatusCodes.LA_E_REVOKED;
                }
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.INVALID_LICENSE_TYPE)
                {
                    LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                    return LexStatusCodes.LA_E_LICENSE_TYPE;
                }
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.METER_ATTRIBUTE_USES_LIMIT_REACHED)
                {
                    return LexStatusCodes.LA_E_METER_ATTRIBUTE_USES_LIMIT_REACHED;
                }
                return LexStatusCodes.LA_E_CLIENT;
            }
            return LexStatusCodes.LA_E_INET;
        }

        public static int DeactivateFromServer(string productId, ActivationPayload activationPayload)
        {
            var httpService = new LexHttpService();
            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = httpService.DeleteActivation(activationPayload.Id);
            }
            catch (Exception exception)
            {
                System.Console.WriteLine(exception.Message);
                return LexStatusCodes.LA_E_INET;
            }
            if (!httpResponse.IsSuccessStatusCode)
            {
                return DeactivationErrorHandler(productId, httpResponse);
            }
            activationPayload.IsValid = false;
            LexDataStore.Reset(productId);
            return LexStatusCodes.LA_OK;
        }

        public static int DeactivationErrorHandler(string productId, HttpResponseMessage httpResponse)
        {
            if (httpResponse.StatusCode == HttpStatusCode.InternalServerError || httpResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                return LexStatusCodes.LA_E_SERVER;
            }
            if (httpResponse.StatusCode == (HttpStatusCode)LexConstants.HttpTooManyRequests)
            {
                return LexStatusCodes.LA_E_RATE_LIMIT;
            }
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                LexDataStore.ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
                return LexStatusCodes.LA_E_ACTIVATION_NOT_FOUND;
            }
            if (httpResponse.StatusCode == HttpStatusCode.Conflict)
            {
                var errorResponse = JsonConvert.DeserializeObject<HttpErrorResponse>(httpResponse.Content.ReadAsStringAsync().Result);
                if (errorResponse.Code == LexConstants.ActivationErrorCodes.DEACTIVATION_LIMIT_REACHED)
                {
                    return LexStatusCodes.LA_E_DEACTIVATION_LIMIT;
                }
            }
            return LexStatusCodes.LA_E_CLIENT;
        }
        public static string GetActivationRequest(string licenseKey, string productId, List<ActivationMetadata> metadata, List<ActivationMeterAttribute> meterAttributes)
        {
            var activationRequest = new ActivationRequest();
            activationRequest.Fingerprint = LexSystemInfo.GetFingerPrint();
            activationRequest.ProductId = productId;
            activationRequest.Key = licenseKey;
            activationRequest.Os = LexSystemInfo.GetOsName();
            activationRequest.OsVersion = LexSystemInfo.GetOsVersion();
            activationRequest.UserHash = LexEncryptionService.Sha256(LexSystemInfo.GetUser());
            activationRequest.AppVersion = LexDataStore.AppVersion;
            activationRequest.ClientVersion = LexDataStore.ClientVersion;
            activationRequest.VmName = LexSystemInfo.GetVmName();
            activationRequest.Hostname = LexSystemInfo.GetHostname();
            activationRequest.Email = String.Empty;
            activationRequest.Password = String.Empty;
            activationRequest.Metadata = metadata;
            activationRequest.MeterAttributes = meterAttributes;
            string jsonBody = JsonConvert.SerializeObject(activationRequest);
            return jsonBody;
        }

        public static string GetMetadata(string key, List<Metadata> metadata)
        {
            string normalizedKey = key.ToUpper();
            foreach (var item in metadata)
            {
                if (normalizedKey == item.Key.ToUpper())
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static LicenseMeterAttribute GetLicenseMeterAttribute(string name, List<LicenseMeterAttribute> meterAttributes)
        {
            string normalizedName = name.ToUpper();
            foreach (var item in meterAttributes)
            {
                if (normalizedName == item.Name.ToUpper())
                {
                    return new LicenseMeterAttribute(name, item.AllowedUses, item.TotalUses);
                }
            }
            return null;
        }

        public static bool MeterAttributeExists(string name, List<LicenseMeterAttribute> meterAttributes)
        {
            string normalizedName = name.ToUpper();
            foreach (var item in meterAttributes)
            {
                if (normalizedName == item.Name.ToUpper())
                {
                    return true;
                }
            }
            return false;
        }

        public static ActivationMeterAttribute GetActivationMeterAttribute(string name, List<ActivationMeterAttribute> meterAttributes)
        {
            string normalizedName = name.ToUpper();
            foreach (var item in meterAttributes)
            {
                if (normalizedName == item.Name.ToUpper())
                {
                    return item;
                }
            }
            return null;
        }
    }
}