
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;

using Cryptlex.Services;
using Cryptlex.Models;

namespace Cryptlex
{
    public class LicenseManager
    {
        private string _productId;
        private string _rsaPublicKey;
        private string _licenseKey;
        private Timer _timer;
        public delegate void CallbackType(int status);
        private CallbackType _callback;
        private ActivationPayload _activationPayload;

        /// <summary>
        /// Sets the RSA public key.
        /// 
        /// This function must be called on every start of your program
        /// before any other functions are called.
        /// </summary>
        /// <param name="path">path of the RSA public key file</param>
        public void SetRsaPublicKey(string path)
        {
            if (String.IsNullOrEmpty(this._productId))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_PRODUCT_ID);
            }
            if (!File.Exists(path))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_FILE_PATH);
            }
            this._rsaPublicKey = File.ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// Sets the product id of your application.
        /// 
        /// This function must be called on every start of your program before
        /// any other functions are called, with the exception of SetProductFile()
        /// or SetProductData() function.
        /// </summary>
        /// <param name="productId">the unique product id of your application as mentioned on the product page in the dashboard</param>
        public void SetProductId(string productId)
        {
            if (!LexValidator.ValidateProductId(productId))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_PRODUCT_ID);
            }
            this._productId = productId;
        }

        /// <summary>
        /// Sets the license key required to activate the license.
        /// </summary>
        /// <param name="licenseKey">a valid license key</param>
        public void SetLicenseKey(string licenseKey)
        {
            if (String.IsNullOrEmpty(this._productId))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_PRODUCT_ID);
            }
            if (String.IsNullOrEmpty(this._rsaPublicKey))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_RSA_PUBLIC_KEY);
            }
            if (!LexValidator.ValidateLicenseKey(licenseKey))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_LICENSE_KEY);
            }
            this._licenseKey = licenseKey;
            LexDataStore.SaveValue(this._productId, LexConstants.KEY_LICENSE_KEY, licenseKey);
        }

        /// <summary>
        /// Sets server sync callback function.
        /// 
        /// Whenever the server sync occurs in a separate thread, and server returns the response,
        /// license callback function gets invoked with the following status codes:
        /// LA_OK, LA_EXPIRED, LA_SUSPENDED, LA_E_REVOKED, LA_E_ACTIVATION_NOT_FOUND,
        /// LA_E_MACHINE_FINGERPRINT, LA_E_AUTHENTICATION_FAILED, LA_E_COUNTRY, LA_E_INET,
        /// LA_E_SERVER, LA_E_RATE_LIMIT, LA_E_IP
        /// </summary>
        /// <param name="callback"></param>
        public void SetLicenseCallback(CallbackType callback)
        {
            if (String.IsNullOrEmpty(this._productId))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_PRODUCT_ID);
            }
            this._callback = callback;
        }

        /// <summary>
        /// Sets the current app version of your application.
        /// 
        /// The app version appears along with the activation details in dashboard. It
        /// is also used to generate app analytics.
        /// </summary>
        /// <param name="appVersion"></param>
        public void SetAppVersion(string appVersion)
        {
            if (String.IsNullOrEmpty(this._productId))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_PRODUCT_ID);
            }
            LexDataStore.AppVersion = appVersion;
        }

        /// <summary>
        /// Gets the license metadata of the license.
        /// </summary>
        /// <param name="key">metadata key to retrieve the value</param>
        /// <returns>Returns the value of metadata for the key.</returns>
        public string GetLicenseMetadata(string key)
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status))
            {
                string value = LexActivationService.GetMetadata(key, _activationPayload.LicenseMetadata);
                if (value == null)
                {
                    throw new LexActivatorException(LexStatusCodes.LA_E_METADATA_KEY_NOT_FOUND);
                }
                return value;
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// Gets the license meter attribute allowed uses and total uses.
        /// </summary>
        /// <param name="name">name of the meter attribute</param>
        /// <returns>Returns the values of meter attribute allowed and total uses.</returns>
        public LicenseMeterAttribute GetLicenseMeterAttribute(string name)
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status))
            {
                var licenseMeterAttribute = LexActivationService.GetLicenseMeterAttribute(name, _activationPayload.LicenseMeterAttributes);
                if (licenseMeterAttribute == null)
                {
                    throw new LexActivatorException(LexStatusCodes.LA_E_METER_ATTRIBUTE_NOT_FOUND);
                }
                return licenseMeterAttribute;
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// Gets the license key used for activation.
        /// </summary>
        /// <returns>Returns the license key.</returns>
        public string GetLicenseKey()
        {
            if (String.IsNullOrEmpty(this._productId))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_PRODUCT_ID);
            }
            string licenseKey = LexDataStore.GetValue(this._productId, LexConstants.KEY_LICENSE_KEY);
            if (String.IsNullOrEmpty(licenseKey))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_LICENSE_KEY);
            }
            return licenseKey;
        }

        /// <summary>
        /// Gets the license expiry date timestamp.
        /// </summary>
        /// <returns>Returns the timestamp.</returns>
        public long GetLicenseExpiryDate()
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status))
            {
                return _activationPayload.ExpiresAt;
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// Gets the email associated with the license user.
        /// </summary>
        /// <returns>Returns the license user email.</returns>
        public string GetLicenseUserEmail()
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status))
            {
                return _activationPayload.Email;
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// Gets the name associated with the license user.
        /// </summary>
        /// <returns>Returns the license user name.</returns>
        public string GetLicenseUserName()
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status))
            {
                return _activationPayload.Name;
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// Gets the company associated with the license user.
        /// </summary>
        /// <returns>Returns the license user company.</returns>
        public string GetLicenseUserCompany()
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status))
            {
                return _activationPayload.Company;
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// Gets the metadata associated with the license user.
        /// </summary>
        /// <param name="key">key to retrieve the value</param>
        /// <returns>Returns the value of metadata for the key.</returns>
        public string GetLicenseUserMetadata(string key)
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status))
            {
                string value = LexActivationService.GetMetadata(key, _activationPayload.UserMetadata);
                if (value == null)
                {
                    throw new LexActivatorException(LexStatusCodes.LA_E_METADATA_KEY_NOT_FOUND);
                }
                return value;
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// Gets the license type (node-locked or hosted-floating).
        /// </summary>
        /// <returns>Returns the license type.</returns>
        public string GetLicenseType()
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status))
            {
                return _activationPayload.Type;
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// Gets the meter attribute uses consumed by the activation.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns the value of meter attribute uses by the activation.</returns>
        public long GetActivationMeterAttributeUses(string name)
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status))
            {
                bool exists = false;
                exists = LexActivationService.MeterAttributeExists(name, _activationPayload.LicenseMeterAttributes);
                if (!exists)
                {
                    throw new LexActivatorException(LexStatusCodes.LA_E_METER_ATTRIBUTE_NOT_FOUND);
                }
                var activationMeterAttribute = LexActivationService.GetActivationMeterAttribute(name, _activationPayload.ActivationMeterAttributes);
                if (activationMeterAttribute == null)
                {
                    return 0;
                }
                return activationMeterAttribute.Uses;
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// Activates the license by contacting the Cryptlex servers. It
        /// validates the key and returns with encrypted and digitally signed token
        /// which it stores and uses to activate your application.
        /// 
        /// This function should be executed at the time of registration, ideally on
        /// a button click.
        /// </summary>
        /// <returns>LA_OK, LA_EXPIRED, LA_SUSPENDED, LA_FAIL</returns>
        public int ActivateLicense()
        {
            if (String.IsNullOrEmpty(this._productId))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_PRODUCT_ID);
            }
            if (String.IsNullOrEmpty(this._rsaPublicKey))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_RSA_PUBLIC_KEY);
            }

            _licenseKey = LexDataStore.GetValue(this._productId, LexConstants.KEY_LICENSE_KEY);
            if (String.IsNullOrEmpty(_licenseKey))
            {
                throw new LexActivatorException(LexStatusCodes.LA_E_LICENSE_KEY);
            }

            _activationPayload = new ActivationPayload();
            var meterAttributes = new List<ActivationMeterAttribute>();
            int status = LexActivationService.ActivateFromServer(_productId, _licenseKey, _rsaPublicKey, _activationPayload, meterAttributes);
            if (LexValidator.ValidateSuccessCode(status))
            {
                StartTimer(_activationPayload.ServerSyncInterval, _activationPayload.ServerSyncInterval);
                return status;
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// Deactivates the license activation and frees up the corresponding activation
        /// slot by contacting the Cryptlex servers.
        /// 
        /// This function should be executed at the time of de-registration, ideally on
        /// a button click.
        /// </summary>
        /// <returns>LA_OK</returns>
        public int DeactivateLicense()
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status))
            {
                status = LexActivationService.DeactivateFromServer(_productId, _activationPayload);
                if (status == LexStatusCodes.LA_OK)
                {
                    return status;
                }
            }
            throw new LexActivatorException(status);
        }

        /// <summary>
        /// It verifies whether your app is genuinely activated or not. The verification is
        /// done locally by verifying the cryptographic digital signature fetched at the time of activation.
        /// 
        /// After verifying locally, it schedules a server check in a separate thread. After the
        /// first server sync it periodically does further syncs at a frequency set for the license.
        /// 
        /// In case server sync fails due to network error, and it continues to fail for fixed
        /// number of days (grace period), the function returns LA_GRACE_PERIOD_OVER instead of LA_OK.
        /// 
        /// This function must be called on every start of your program to verify the activation
        /// of your app.
        /// </summary>
        /// <returns>LA_OK, LA_EXPIRED, LA_SUSPENDED, LA_GRACE_PERIOD_OVER, LA_FAIL</returns>
        public int IsLicenseGenuine()
        {
            int status = IsLicenseValid();
            if (LexValidator.ValidateSuccessCode(status) && _activationPayload.ServerSyncInterval != 0)
            {
                StartTimer(LexConstants.SERVER_SYNC_DELAY, _activationPayload.ServerSyncInterval);
            }
            switch (status)
            {
                case LexStatusCodes.LA_OK:
                    return LexStatusCodes.LA_OK;
                case LexStatusCodes.LA_EXPIRED:
                    return LexStatusCodes.LA_EXPIRED;
                case LexStatusCodes.LA_SUSPENDED:
                    return LexStatusCodes.LA_SUSPENDED;
                case LexStatusCodes.LA_GRACE_PERIOD_OVER:
                    return LexStatusCodes.LA_GRACE_PERIOD_OVER;
                case LexStatusCodes.LA_FAIL:
                    return LexStatusCodes.LA_FAIL;
                default:
                    throw new LexActivatorException(status);
            }
        }

        /// <summary>
        /// It verifies whether your app is genuinely activated or not. The verification is
        /// done locally by verifying the cryptographic digital signature fetched at the time of activation.
        /// 
        /// This is just an auxiliary function which you may use in some specific cases, when you
        /// want to skip the server sync.
        /// 
        /// NOTE: You may want to set grace period to 0 to ignore grace period.
        /// </summary>
        /// <returns>LA_OK, LA_EXPIRED, LA_SUSPENDED, LA_GRACE_PERIOD_OVER, LA_FAIL</returns>
        private int IsLicenseValid()
        {
            if (String.IsNullOrEmpty(this._productId))
            {
                return LexStatusCodes.LA_E_PRODUCT_ID;
            }
            if (!LexValidator.ValidateSystemTime(this._productId))
            {
                return LexStatusCodes.LA_E_TIME_MODIFIED;
            }
            _licenseKey = LexDataStore.GetValue(this._productId, LexConstants.KEY_LICENSE_KEY);
            if (!LexValidator.ValidateLicenseKey(_licenseKey))
            {
                return LexStatusCodes.LA_E_LICENSE_KEY;
            }
            string jwt = LexDataStore.GetValue(_productId, LexConstants.KEY_ACTIVATION_JWT);
            if (String.IsNullOrEmpty(jwt))
            {
                return LexStatusCodes.LA_FAIL;
            }
            if (_activationPayload != null && _activationPayload.IsValid)
            {
                return LexValidator.ValidateActivationStatus(_productId, _activationPayload);
            }
            _activationPayload = new ActivationPayload();
            return LexValidator.ValidateActivation(jwt, _rsaPublicKey, _licenseKey, _productId, _activationPayload);
        }

        /// <summary>
        /// Increments the meter attribute uses of the activation.
        /// </summary>
        /// <param name="name">name of the meter attribute</param>
        /// <param name="increment">the increment value</param>
        public void IncrementActivationMeterAttributeUses(string name, uint increment)
        {
            long currentUses = GetActivationMeterAttributeUses(name);
            long uses = currentUses + increment;
            List<ActivationMeterAttribute> meterAttributes = _activationPayload.ActivationMeterAttributes;
            int status = UpdateMeterAttributeUses(name, meterAttributes, uses);
            if (!LexValidator.ValidateSuccessCode(status))
            {
                throw new LexActivatorException(status);
            }
        }

        /// <summary>
        /// Decrements the meter attribute uses of the activation.
        /// </summary>
        /// <param name="name">name of the meter attribute</param>
        /// <param name="decrement">the decrement value</param>
        public void DecrementActivationMeterAttributeUses(string name, uint decrement)
        {
            long currentUses = GetActivationMeterAttributeUses(name);
            if (decrement > currentUses)
            {
                decrement = (uint)currentUses;
            }
            long uses = currentUses - decrement;
            List<ActivationMeterAttribute> meterAttributes = _activationPayload.ActivationMeterAttributes;
            int status = UpdateMeterAttributeUses(name, meterAttributes, uses);
            if (!LexValidator.ValidateSuccessCode(status))
            {
                throw new LexActivatorException(status);
            }
        }

        /// <summary>
        /// Resets the meter attribute uses consumed by the activation.
        /// </summary>
        /// <param name="name">name of the meter attribute</param>
        public void ResetActivationMeterAttributeUses(string name)
        {
            long currentUses = GetActivationMeterAttributeUses(name);
            List<ActivationMeterAttribute> meterAttributes = _activationPayload.ActivationMeterAttributes;
            int status = UpdateMeterAttributeUses(name, meterAttributes, 0);
            if (!LexValidator.ValidateSuccessCode(status))
            {
                throw new LexActivatorException(status);
            }
        }

        private int UpdateMeterAttributeUses(string name, List<ActivationMeterAttribute> meterAttributes, long uses)
        {
            string normalizedName = name.ToUpper();
            bool exists = false;
            foreach (var item in meterAttributes)
            {
                if (normalizedName == item.Name.ToUpper())
                {
                    item.Uses = uses; ;
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                meterAttributes.Add(new ActivationMeterAttribute(name, uses));
            }

            int status = LexActivationService.ActivateFromServer(_productId, _licenseKey, _rsaPublicKey, _activationPayload, meterAttributes, true);
            return status;
        }

        private void LicenseTimerCallback(Object stateInfo)
        {
            if (_activationPayload.IsValid == false)   // invalid as license was dropped
            {
                StopTimer();
                return;
            }
            var meterAttributes = new List<ActivationMeterAttribute>();
            int status = LexActivationService.ActivateFromServer(_productId, _licenseKey, _rsaPublicKey, _activationPayload, meterAttributes, true);
            if (!LexValidator.ValidateServerSyncAllowedStatusCodes(status))
            {
                StopTimer();
                this._callback(status);
                return;
            }
            this._callback(status);
        }

        private void StartTimer(long dueTime, long interval)
        {
            if (_callback != null)
            {
                if(_timer != null)
                {
                    return;
                }
                _timer = new Timer(LicenseTimerCallback, null, dueTime * 1000, interval * 1000);
            }
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}