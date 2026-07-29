using System;
using Microsoft.Xrm.Sdk;

namespace D365.Plugins
{
    public class ContactUpdatePlugin : IPlugin
    {
        private const string DescriptionAttribute = "description";

        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracing?.Trace("ContactUpdatePlugin: Enter Execute");

            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (context == null) return;

            if (!string.Equals(context.MessageName, "Update", StringComparison.OrdinalIgnoreCase)) return;
            if (!string.Equals(context.PrimaryEntityName, "contact", StringComparison.OrdinalIgnoreCase)) return;
            if (context.Stage != 20) return;

            if (context.Depth > 1)
            {
                tracing?.Trace("ContactUpdatePlugin: Skipping because Depth > 1");
                return;
            }

            if (!(context.InputParameters?.Contains("Target") == true && context.InputParameters["Target"] is Entity entity)) return;

            try
            {
                var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                entity[DescriptionAttribute] = currentDateTime;

                tracing?.Trace($"ContactUpdatePlugin: Set description to {currentDateTime}");
            }
            catch (Exception ex)
            {
                tracing?.Trace("ContactUpdatePlugin failed: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("ContactUpdatePlugin failed. See trace for details.", ex);
            }
        }
    }
}
