using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace basic_site
{
    public class MyTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            var requestTelemetry = telemetry as RequestTelemetry;
            // Is this a TrackRequest() ?
            if (requestTelemetry == null) return;
            int code;
            bool parsed = Int32.TryParse(requestTelemetry.ResponseCode, out code);
            if (!parsed) return;
            if (code >= 400 && code < 500)
            {
                // If we set the Success property, the SDK won't change it:
                requestTelemetry.Success = true;

                // Allow us to filter these requests in the portal:
                requestTelemetry.Properties["Overridden400s"] = "true";
            }
            // else leave the SDK to set the Success property
        }
    }

    public class HttpContextRequestTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpContextRequestTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor =
                httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void Initialize(ITelemetry telemetry)
        {
            var requestTelemetry = telemetry as RequestTelemetry;
            if (requestTelemetry == null) return;

            var claims = this.httpContextAccessor.HttpContext.User.Claims;
            Claim oidClaim = claims.FirstOrDefault(claim => claim.Type == "oid");
            requestTelemetry.Properties.Add("UserOid", oidClaim?.Value);
        }
    }
}
