using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace WeiXinApi.Models
{
    public class RequestContainer
    {
        /// <summary>
        /// HttpWebRequest异步请求完成事件的委托。
        /// </summary>
        /// <param name="response">异步请求返回的HttpWebResponse实例。</param>
        public delegate void AsyncRequestCompletedEventHandler(HttpWebResponse response);

        /// <summary>
        /// 当异步请求完成后引发该事件。
        /// </summary>
        public event AsyncRequestCompletedEventHandler AsyncRequestCompletedEvent;

        /// <summary>
        /// 用于存储请求的数据。
        /// </summary>
        private byte[] requestContentBytes;

        /// <summary>
        /// 获取当前的HttpWebRequest对象。
        /// </summary>
        public HttpWebRequest RequestObj { get; private set; }

        /// <summary>
        /// 获取或设置当前请求的字符编码。
        /// </summary>
        public Encoding RequestEncoding { get; set; }

        /// <summary>
        /// 获取或设置当前响应体的字符编码。
        /// </summary>
        public Encoding ResponseEncoding { get; set; }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="requestUri">请求的Uri。</param>
        public RequestContainer(Uri requestUri)
        {
            HttpWebRequest request = this.CreateRequest(requestUri);
            this.Initialize(request);
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="requestUrl">请求的Url。</param>
        public RequestContainer(string requestUrl)
        {
            Uri requestUri = new Uri(requestUrl);
            HttpWebRequest request = this.CreateRequest(requestUri);
            this.Initialize(request);
        }

        /// <summary>
        /// 设置当前请求的内容。
        /// </summary>
        /// <param name="content">请求内容的字节数组。</param>
        public void SetRequestContent(byte[] content)
        {
            this.requestContentBytes = content;
        }

        /// <summary>
        /// 设置当前请求的内容。
        /// </summary>
        /// <param name="content">请求内容字符串。</param>
        public void SetRequestContent(string content)
        {
            byte[] contentBytes = this.RequestEncoding.GetBytes(content);
            this.requestContentBytes = contentBytes;
        }

        /// <summary>
        /// 获取资源响应。获取完毕后 关闭
        /// </summary>
        /// <returns>HttpWebResponse实例。</returns>
        public HttpWebResponse GetResponse()
        {

            HttpWebResponse response = null;
            System.GC.Collect();
            Stream inStream = null;
            try
            {
                if (this.requestContentBytes != null)
                {
                    int length = this.requestContentBytes.Length;

                    if (length > 0)
                    {
                        this.RequestObj.ContentLength = length;

                        using (inStream = this.RequestObj.GetRequestStream())
                        {
                            inStream.Write(this.requestContentBytes, 0, length);

                        }
                    }
                }
                response = this.RequestObj.GetResponse() as HttpWebResponse;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
            finally
            {
                if (inStream != null)
                {
                    inStream.Close();
                }

            }
            return response;
        }

        private string GetBadRequestErrorMessage(WebException webException)
        {
            HttpWebResponse response = (HttpWebResponse)webException.Response;

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                using (Stream errData = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(errData))
                    {
                        string errorMessage = reader.ReadToEnd();
                        return errorMessage;
                    }
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// 获取资源响应。
        /// </summary>
        /// <param name="response">HttpWebResponse实例。</param>
        /// <returns>资源响应内容。</returns>
        public string GetResponseContent(HttpWebResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader streamReader = new StreamReader(responseStream, this.ResponseEncoding))
                {
                    string responseContent = streamReader.ReadToEnd();
                    return responseContent;
                }
            }
        }

        /// <summary>
        /// 获取资源响应。
        /// </summary>
        /// <returns>资源响应内容。</returns>
        public string GetResponseContent()
        {
            HttpWebResponse response = this.GetResponse();
            string responseContent = this.GetResponseContent(response);
            return responseContent;
        }

        /// <summary>
        /// 异步请求。
        /// </summary>
        public void RequestAsync()
        {
            if (this.requestContentBytes != null)
            {
                int length = this.requestContentBytes.Length;

                if (length > 0)
                {
                    this.RequestObj.ContentLength = length;
                    this.RequestObj.BeginGetRequestStream(new AsyncCallback(AsyncRequestStreamCallBack), this.RequestObj);
                }
            }

            this.RequestObj.BeginGetResponse(new AsyncCallback(AsyncResponseCallBack), this.RequestObj);
        }


        /// <summary>
        /// 创建HttpWebRequest实例。
        /// </summary>
        /// <param name="requestUri">请求的URI。</param>
        /// <returns>HttpWebRequest实例。</returns>
        private HttpWebRequest CreateRequest(Uri requestUri)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException("uri");
            }

            HttpWebRequest request = null;

            if (requestUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(requestUri) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(requestUri) as HttpWebRequest;
            }

            return request;
        }

        /// <summary>
        /// 进行初始化工作。
        /// </summary>
        /// <param name="request">HttpWebRequest实例。</param>
        private void Initialize(HttpWebRequest request)
        {
            this.RequestObj = request;
            this.RequestEncoding = Encoding.UTF8;
            this.ResponseEncoding = Encoding.UTF8;
        }

        /// <summary>
        /// 异步请求回调函数。
        /// </summary>
        /// <param name="asyncResult">返回的异步请求操作结果</param>
        private void AsyncRequestStreamCallBack(IAsyncResult asyncResult)
        {
            HttpWebRequest asyncRequest = (HttpWebRequest)asyncResult.AsyncState;

            using (Stream requestStream = asyncRequest.EndGetRequestStream(asyncResult))
            {
                requestStream.Write(this.requestContentBytes, 0, this.requestContentBytes.Length);
            }

            asyncRequest.BeginGetResponse(new AsyncCallback(AsyncResponseCallBack), asyncRequest);
        }

        /// <summary>
        /// 异步请求回调函数。
        /// </summary>
        /// <param name="asyncResult">返回的异步请求操作结果</param>
        private void AsyncResponseCallBack(IAsyncResult asyncResult)
        {
            HttpWebRequest asyncRequest = asyncResult.AsyncState as HttpWebRequest;
            HttpWebResponse asyncResponse = asyncRequest.EndGetResponse(asyncResult) as HttpWebResponse;
            this.OnAsyncRequestCompletedEvent(asyncResponse);
        }

        /// <summary>
        /// 引发AsyncRequestCompletedEvent事件。
        /// </summary>
        /// <param name="response">异步请求返回的HttpWebResponse实例。</param>
        private void OnAsyncRequestCompletedEvent(HttpWebResponse response)
        {
            if (AsyncRequestCompletedEvent != null)
            {
                AsyncRequestCompletedEvent(response);
            }
        }

        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}