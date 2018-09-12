using System;
using System.Diagnostics;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;
using YW.Data;

namespace YW.Server
{
    internal class IncomingMessageLoggerInspector : IDispatchMessageInspector
    {
        #region IDispatchMessageInspector Members

        public IncomingMessageLoggerInspector()
        {
        }


        public object AfterReceiveRequest(
            ref Message request,
            IClientChannel channel,
            InstanceContext instanceContext)
        {
            var context = OperationContext.Current;
            if (context == null) return null;

            var operationName = ParseOperationName(context.IncomingMessageHeaders.Action);

//            Logger.Info($"Operation [{operationName}] started,ContractName:{context.EndpointDispatcher.ContractName}");

            return MarkStartOfOperation(context.EndpointDispatcher.ContractName, operationName, context.SessionId);
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var context = OperationContext.Current;
            if (context == null) return;
            var operationName = ParseOperationName(context.IncomingMessageHeaders.Action);

            MarkEndOfOperation(context.EndpointDispatcher.ContractName, operationName, context.SessionId, correlationState);
        }

        #endregion

        #region Private Methods

        private string ParseOperationName(string action)
        {
            if (string.IsNullOrEmpty(action)) return action;

            string actionName = action;

            int index = action.LastIndexOf('/');
            if (index >= 0)
            {
                actionName = action.Substring(index + 1);
            }

            return actionName;
        }

        private object MarkStartOfOperation(string inspectorType, string operationName, string sessionId)
        {
            return Stopwatch.StartNew();
        }

        private void MarkEndOfOperation(
            string inspectorType, string operationName,
            string sessionId, object correlationState)
        {
            var watch = (Stopwatch) correlationState;
            watch.Stop();
            if (watch.ElapsedMilliseconds > 100 && !string.IsNullOrEmpty(operationName) && !operationName.Equals("GetAddress") && !operationName.Equals("WIFILBS"))
            {
                var message = string.Format(CultureInfo.InvariantCulture,
                    "Operation [{0}] returned after [{1}] milliseconds at [{2}] on [{3}] in thread [{4}].",
                    operationName, watch.ElapsedMilliseconds, inspectorType,
                    DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture),
                    Thread.CurrentThread.ManagedThreadId);
                Logger.Info(message);
            }
        }

        #endregion
    }
}