using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace basic_site
{

    public class SuccessfulDependencyFilter : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        // next will point to the next TelemetryProcessor in the chain.
        public SuccessfulDependencyFilter(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        public void Process(ITelemetry item)
        {
            // To filter out an item, return without calling the next processor.
            if (!OKtoSend(item)) { return; }

            this.Next.Process(item);
        }

        // Example: replace with your own criteria.
        private bool OKtoSend(ITelemetry item)
        {
            var dependency = item as DependencyTelemetry;
            if (dependency == null) return true;

            return dependency.Success != true;
        }
    }

    public class SynthenicSourceFilter : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }
        public SynthenicSourceFilter(ITelemetryProcessor next)
        {
            this.Next = next;
        }
        public void Process(ITelemetry item)
        {
            if (!string.IsNullOrEmpty(item.Context.Operation.SyntheticSource)) { return; }

            // Send everything else:
            this.Next.Process(item);
        }
    }

    public class FailedAuthenticationFilter : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }
        public FailedAuthenticationFilter(ITelemetryProcessor next)
        {
            this.Next = next;
        }
        public void Process(ITelemetry item)
        {
            var request = item as RequestTelemetry;

            if (request != null &&
            request.ResponseCode.Equals("401", StringComparison.OrdinalIgnoreCase))
            {
                // To filter out an item, return without calling the next processor.
                return;
            }

            // Send everything else
            this.Next.Process(item);
        }
    }

    public class FastDependencyCallFilter : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }
        public FastDependencyCallFilter(ITelemetryProcessor next)
        {
            this.Next = next;
        }
        public void Process(ITelemetry item)
        {
            var request = item as DependencyTelemetry;

            if (request != null && request.Duration.TotalMilliseconds < 100)
            {
                return;
            }
            this.Next.Process(item);
        }
    }
}
