using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote
{
    public static class StatusCodeHelper
    {
        public static bool IsInformationalResponse(this HttpStatusCode statusCode)
        {
            int statusCodeAsInt = (int)statusCode;
            return statusCodeAsInt >= 100 && statusCodeAsInt <= 199;
        }

        public static bool IsSuccess(this HttpStatusCode statusCode)
        {
            int statusCodeAsInt = (int)statusCode;
            return statusCodeAsInt >= 200 && statusCodeAsInt <= 299;
        }

        public static bool IsRedirection(this HttpStatusCode statusCode)
        {
            int statusCodeAsInt = (int)statusCode;
            return statusCodeAsInt >= 300 && statusCodeAsInt <= 399;
        }

        public static bool IsClientError(this HttpStatusCode statusCode)
        {
            int statusCodeAsInt = (int)statusCode;
            return statusCodeAsInt >= 400 && statusCodeAsInt <= 499;
        }

        public static bool IsServerError(this HttpStatusCode statusCode)
        {
            int statusCodeAsInt = (int)statusCode;
            return statusCodeAsInt >= 500 && statusCodeAsInt <= 599;
        }
    }
}
