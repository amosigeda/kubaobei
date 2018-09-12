using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using YW.Model.Entity;

namespace YW.Contracts
{
    [ServiceContract]
    public interface IFile
    {
        [OperationContract, WebGet(UriTemplate = "GetImage?path={path}")]
        Stream GetImage(string path);

        [OperationContract, WebGet(UriTemplate = "GetAMR?path={path}")]
        Stream GetAMR(string path);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        Message BillList(BillListJson data);
    }
}