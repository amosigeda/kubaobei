using System;
using System.IO;
using System.IO.Compression;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using Newtonsoft.Json;
using YW.Contracts;
using YW.Data;
using YW.Model.Entity;

namespace YW.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple,AddressFilterMode = AddressFilterMode.Any)]
    public class GFile : IFile
    {
        public Stream GetImage(string path)
        {
            #region 获取文件

            FileStream fs = null;
            try
            {
                if (path.Length > 4 && path.Substring(path.Length - 3, 3).ToLower().Equals("jpg") && path.IndexOf("..", StringComparison.Ordinal) < 0)
                {
                    var pth = Config.GetInstance().Path + "\\Upload\\" + path;
                    if (!File.Exists(pth))
                    {
                        return null;
                    }
                    using (fs = File.OpenRead(pth))
                    {
                        int length = (int) fs.Length;
                        byte[] data = new byte[length];
                        fs.Position = 0;
                        fs.Read(data, 0, length);
                        bool gzip = false;
                        MemoryStream ms;
                        foreach (var item in WebOperationContext.Current.IncomingRequest.Headers)
                        {
                            if (item.ToString().ToLower().Equals("accept-encoding"))
                            {
                                if (WebOperationContext.Current.IncomingRequest.Headers[item.ToString()].IndexOf("gzip", StringComparison.Ordinal) > -1)
                                {
                                    gzip = true;
                                }

                                break;
                            }
                        }

                        if (gzip)
                        {
                            ms = new MemoryStream();
                            using (GZipStream gzStream = new GZipStream(ms, CompressionMode.Compress, true))
                            {
                                gzStream.Write(data, 0, data.Length);
                            }

                            byte[] compressedBytes = ms.ToArray();
                            ms = new MemoryStream(compressedBytes);
                            WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Encoding", "gzip");
                        }
                        else
                        {
                            ms = new MemoryStream(data);
                        }

                        WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpg";
                        return ms;
                    }
                }
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                //Data.Logger.Error(ex);
            }
            finally
            {
                if (fs != null)
                {
                    try
                    {
                        fs.Close();
                        fs.Dispose();
                    }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                    catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                    {
                        //
                    }
                }
            }

            return null;

            #endregion
        }

        public Stream GetAMR(string path)
        {
            #region 获取语音

            FileStream fs = null;
            try
            {
                if (path.Length > 4 && path.IndexOf("..", StringComparison.Ordinal) < 0)
                {
                    var pth = Config.GetInstance().Path + "\\AMR\\" + path;
                    if (!File.Exists(pth))
                    {
                        return null;
                    }

                    using (fs = File.OpenRead(pth))
                    {
                        int length = (int) fs.Length;
                        byte[] data = new byte[length];
                        fs.Position = 0;
                        fs.Read(data, 0, length);
                        bool gzip = false;
                        MemoryStream ms;
                        foreach (var item in WebOperationContext.Current.IncomingRequest.Headers)
                        {
                            if (item.ToString().ToLower().Equals("accept-encoding"))
                            {
                                if (WebOperationContext.Current.IncomingRequest.Headers[item.ToString()].IndexOf("gzip", StringComparison.Ordinal) > -1)
                                {
                                    gzip = true;
                                }

                                break;
                            }
                        }

                        if (gzip)
                        {
                            ms = new MemoryStream();
                            using (GZipStream gzStream = new GZipStream(ms, CompressionMode.Compress, true))
                            {
                                gzStream.Write(data, 0, data.Length);
                            }

                            byte[] compressedBytes = ms.ToArray();
                            ms = new MemoryStream(compressedBytes);
                            WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Encoding", "gzip");
                        }
                        else
                        {
                            ms = new MemoryStream(data);
                        }

                        WebOperationContext.Current.OutgoingResponse.ContentType = "audio/amr";
                        return ms;
                    }
                }
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                //Data.Logger.Error(ex);
            }
            finally
            {
                if (fs != null)
                {
                    try
                    {
                        fs.Close();
                        fs.Dispose();
                    }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                    catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                    {
                        //
                    }
                }
            }

            return null;

            #endregion
        }

        public Message BillList(BillListJson data)
        {
            #region 接收账单

            string json=JsonConvert.SerializeObject(data);
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/json";
            return WebOperationContext.Current.CreateTextResponse(json);

            #endregion
        }
    }
}