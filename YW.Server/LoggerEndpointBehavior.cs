using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace YW.Server
{
    class LoggerEndpointBehavior : IEndpointBehavior
    {
        #region IEndpointBehavior Members

        public void AddBindingParameters(
          ServiceEndpoint endpoint,
          BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(
          ServiceEndpoint endpoint,
          ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(
          ServiceEndpoint endpoint,
          EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(
                new IncomingMessageLoggerInspector());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}