using Management_platf.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace WeiXinApi.Models
{
    public class MyUtility
    {
        public static T GetObjFromJson<T>(string strJson)
        {
            //T obj = Activator.CreateInstance<T>();
            //using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strJson)))
            //{
            //    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(obj.GetType());
            //    return (T)jsonSerializer.ReadObject(ms);
            //}
            return JsonConvert.DeserializeObject<T>(strJson);
        }

        public static string GetPage(string requestUrl)
        {
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(requestUrl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "GET"; //请求方式GET或POST
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add("Authorization", "Basic YWRtaW46YWRtaW4=");

                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, Encoding.UTF8);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        //<summary>  
        //将对象转化为Json字符串   
        //</summary>  
        //<typeparam name="T">源类型<peparam>  
        //<param name="obj">源类型实例</param>  
        //<returns>Json字符串</returns>  
        public static string GetJsonFromObj(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        /// <summary>
        /// 写本地文件日志
        /// </summary>
        /// <param name="sError"></param>
        /// <param name="ErrorLevel">错误级别</param>
        public static void WriterLog(string pMessage, int ErrorLevel = 1)
        {
            //文件日志
            System.IO.StreamWriter mWriter = null;
            string mMainPath = GetExecutingAssemblyLocation() + @"\Log\";
            string mFileFullName = mMainPath + DateTime.Now.ToString("yyyyMMdd") + ".LOG";
            try
            {
                if (!System.IO.Directory.Exists(mMainPath))
                    System.IO.Directory.CreateDirectory(mMainPath);

                mWriter = new System.IO.StreamWriter(mFileFullName, true);
                mWriter.WriteLine(DateTime.Now.ToString() + "|" + pMessage + "| Level:" + ErrorLevel);
            }
            catch (Exception ex)
            {

                //System.Windows.Forms.MessageBox.Show("WriteLog函数发生错误：" + ex.Message, "ERROR", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            finally
            {
                if (mWriter != null)
                    mWriter.Close();
            }
        }

        /// <summary>
        /// 获取当前DLL文件的本地路径
        /// </summary>
        /// <returns></returns>
        public static string GetExecutingAssemblyLocation()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }


        public static string GetGroupAccess() {
            if (CacheHelper.GetCache("Myaccess2") == null)
            {
                string ID = "wx811b855e73c9b606";
                string SECRECT = "2YavDWF3xeUen6Vs-h6W5pXpJdU0frfmOE7MgNCLvXw";
                RequestContainer re = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid=" + ID + "&corpsecret=" + SECRECT);
                HttpWebResponse res = re.GetResponse();
                string json = re.GetResponseContent();
                dynamic obj = GetObjFromJson<dynamic>(json);
                CacheHelper.SetCache("Myaccess2", obj.access_token, 3600);
                return obj.access_token;
            }
            else
            {
                return CacheHelper.GetCache("Myaccess2").ToString();
            }
        }
    }
}