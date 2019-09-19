using System;

using Cryptlex.Services;

namespace Cryptlex
{
    public class LexConstants
    {
        public const int ALLOWED_CLOCK_OFFSET = 3800;
        public const int MIN_PRODUCT_KEY_LENGTH = 16;
        public const int MAX_PRODUCT_KEY_LENGTH = 256;
        public const int SERVER_SYNC_DELAY = 2;
        public const string JsonContentType = "application/json";
        public const string HttpMethodPatch = "PATCH";
        public const int HttpTooManyRequests =429;



        public const string KEY_LAST_RECORDED_TIME = "KLTR";
        public const string KEY_ACTIVATION_JWT = "KAJ";
        public const string KEY_LICENSE_KEY = "KLK";

        public static class ActivationErrorCodes
        {
            public const string NO_ERROR = "NO_ERROR";
            public const string INVALID_ACTIVATION_FINGERPRINT = "INVALID_ACTIVATION_FINGERPRINT";
            public const string ACTIVATION_LIMIT_REACHED = "ACTIVATION_LIMIT_REACHED";
            public const string METER_ATTRIBUTE_USES_LIMIT_REACHED = "METER_ATTRIBUTE_USES_LIMIT_REACHED";
            public const string AUTHENTICATION_FAILED = "AUTHENTICATION_FAILED";
            public const string DEACTIVATION_LIMIT_REACHED = "DEACTIVATION_LIMIT_REACHED";
            public const string INVALID_PRODUCT_ID = "INVALID_PRODUCT_ID";
            public const string INVALID_LICENSE_KEY = "INVALID_LICENSE_KEY";
            public const string INVALID_LICENSE_TYPE = "INVALID_LICENSE_TYPE";
            public const string VM_ACTIVATION_NOT_ALLOWED = "VM_ACTIVATION_NOT_ALLOWED";
            public const string REVOKED_LICENSE = "REVOKED_LICENSE";
            public const string COUNTRY_NOT_ALLOWED = "COUNTRY_NOT_ALLOWED";
            public const string IP_ADDRESS_NOT_ALLOWED = "IP_ADDRESS_NOT_ALLOWED";
            public const string INVALID_OFFLINE_REQUEST = "INVALID_OFFLINE_REQUEST";
            public const string TRIAL_NOT_ALLOWED = "TRIAL_NOT_ALLOWED";
            public const string TRIAL_ACTIVATION_LIMIT_REACHED = "TRIAL_ACTIVATION_LIMIT_REACHED";

        }

    }
}