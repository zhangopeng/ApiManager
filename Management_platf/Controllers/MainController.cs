using ApiServerLayer.Models;
using Management_platf.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WeiXinApi.Models;

namespace Management_platf.Controllers
{
    public class MainController : Controller
    {
        private static string Tm="T_IMPORT1";
        public static Service s=new Service(Tm);
        // GET: Main
        public ActionResult Index()
        {
            try
            {
                if (HttpContext.Session["UserName"] == null)
                {
                    return View("Login");
                }
                else
                {
                    string a = HttpContext.Session["UserName"].ToString();
                    HttpCookie cookie = new HttpCookie("Login");
                    cookie.Value = a;
                    HttpContext.Response.Cookies.Add(cookie);
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                return View("Login");
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult main()
        {
            return View();
        }
        public ActionResult log()
        {
            return View();
        }
        public void TestGet()
        {
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", "attachment;filename=Sample.xlsx");
            string filename = Server.MapPath("/File/Sample.xlsx");
            Response.TransmitFile(filename);
        }
        public string GetTest()
        {
            try
            {
                int a = Request.ContentLength;
                HttpPostedFileBase up1 = Request.Files["taskFile"];
                if (up1.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string m = up1.FileName;
                    string[] n = m.Split('.');
                    string Date_str = DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒") + ".";
                    //up1.GetType();
                    up1.SaveAs(@"D:\" + Date_str + n[1]);
                    return "ok";
                }
                else {
                    return "上传文件格式不正确！";
                }
            }
            catch (Exception ex) {
                return "Errow:"+ex.Message;
            }
        }
        public string Mylogin(string UserName, string PassWord)
        {
            MessageApi mm = new MessageApi();
            try
            {
                if (s == null)
                {
                    s = new Service(Tm);
                }
                DataTable dll = s.sqlhelper.GetDataTable("select * from " + "APISERVER" + ".users where \"USERNAME\"='" + UserName + "' and \"PASSWORD\"='" + PassWord + "'");
                if (dll.Rows.Count == 0)
                {
                    mm.code = "201";
                    mm.txt = "账号或密码错误！！";
                    return MyUtility.GetJsonFromObj(mm);
                }
                else
                {
                    HttpContext.Session["UserName"] = dll.Select("1=1 ")[0]["USERNAME"].ToString();
                    string a = HttpContext.Session["UserName"].ToString();
                    mm.code = "0";
                    mm.txt = "登陆成功！！";
                    return MyUtility.GetJsonFromObj(mm);
                }
            }
            catch (Exception ex)
            {
                mm.code = "400";
                mm.txt = ex.Message;
                return MyUtility.GetJsonFromObj(mm);
            }
        }

        public string GetXml()
        {
            //Exist("13183489320");
            DataSet dt = ExcelToDS("D:\\Sample.xlsx");
            MessageApi mm = new MessageApi();
            string erro = "";
           // Exist_Tag("羽毛球爱好者/管理/测试", "19909096219");
            //Dictionary<string, string> dict = new Dictionary<string, string>();
            //dynamic de = MyUtility.GetObjFromJson<dynamic>(Json_d); 
            //List<List<string>> sx = new List<List<string>>();
            //List<string> m = new List<string>();
            for (int i = 6; i < dt.Tables[0].Rows.Count; i++)
            {
                mm = Exist_Par(dt.Tables[0].Rows[i][0].ToString());
                if (mm.code == "404")
                {
                    erro += "用户id为" + dt.Tables[0].Rows[i][3].ToString() + "录入失败：" + mm.txt + ";";
                    continue;
                }
                else {
                    mm=createUser(mm.txt, dt.Tables[0].Rows[i][4].ToString(), dt.Tables[0].Rows[i][3].ToString(), dt.Tables[0].Rows[i][2].ToString());
                    if (mm.code == "404")
                    {
                        erro += "用户id为" + dt.Tables[0].Rows[i][3].ToString() + "录入失败：" + mm.txt + ";";
                        continue;
                    }
                    else {
                        Exist_Tag(dt.Tables[0].Rows[i][1].ToString(), dt.Tables[0].Rows[i][3].ToString());
                    }
                }

            }
            //string data = "";
            //Tag t = new Tag();
            //t.tagid = "28";
            //t.userlist = m;
            //data = MyUtility.GetJsonFromObj(t);
            //string accss = MyUtility.GetGroupAccess();
            //RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/tag/addtagusers?access_token=" + accss);
            //req.RequestObj.Method = "POST";
            //req.RequestObj.ContentType = "application/Json";
            //req.SetRequestContent(data);
            //HttpWebResponse rep = req.GetResponse();
            //string x = req.GetResponseContent(rep);
            return erro;
        }
        public string GetMessage(string msg_signature, string timestamp, string nonce, string echostr)
        {
            string postString = string.Empty;
            if (Request.HttpMethod == "POST")
            {
                using (Stream stream = Request.InputStream)
                {
                    Byte[] postBytes = new Byte[stream.Length];
                    stream.Read(postBytes, 0, (Int32)stream.Length);
                    postString = Encoding.UTF8.GetString(postBytes);
                }

                if (!string.IsNullOrEmpty(postString))
                {
                    Write("D:\\Test.txt", postString);
                }
                return "";
            }
            else
            {
                string ss = Auth(msg_signature, timestamp, nonce, echostr);
                return ss;
            }
        }
        public string GetTree()
        {
            string a = MyUtility.GetJsonFromObj(GetSapTree());
            return a;
        }
        public string GetUser(string id)
        {
            string a = getUser(id);
            return a;
        }
        public string GetAllUser(string pageNumber,string pageSize,string id,string Search) {
            string m = "";
            if (Search == null)
            {
                 m = getSapUser(id, pageSize, pageNumber,"");
            }
            else {
                m = getSapUser(id, pageSize, pageNumber, Search);
            }
            return m;
        }
        //public string GetAllUser() {
        //    return "";
        //}



        #region 私有方法
        private static List<Department> GetSapTree() {
            List<Department> department = new List<Department>();
            if (s == null)
            {
                s = new Service(Tm);
            }
            DataTable dll = s.sqlhelper.GetDataTable("select \"BPLName\" from " + s.companyDb + ".OBPL");
            for(int i=0;i< dll.Rows.Count; i++)
            {
                Department de = new Department();
                de.id =Convert.ToString(i + 2);
                de.name=dll.Select("1=1 ")[i]["BPLName"].ToString();
                de.parentid = "1";
                department.Add(de);
                if (dll.Rows.Count==(i+1)) {
                    Department dem = new Department();
                    dem.id = "1";
                    dem.name = "铁骑力士";
                    dem.parentid = "0";
                    department.Add(dem);
                }
            }
                return department;
        }
        private static string getSapUser(string id,string Size,string From,string Search) {
            if (s == null)
            {
                s = new Service(Tm);
            }
            Content cn=new Models.Content();
            DataTable dll = s.sqlhelper.GetDataTable("call "+s.companyDb+".\"U_PD_ZpGetUser\"('"+id+"',"+Size+","+From+",'"+Search+"')");
            if (dll.Rows.Count > 0)
            {
                for (int i = 0; i < dll.Rows.Count; i++)
                {
                    SapUser sa = new SapUser();
                    sa.CardCode = dll.Select("1=1 ")[i]["CardCode"].ToString();
                    sa.CardName = dll.Select("1=1 ")[i]["CardName"].ToString();
                    sa.Note = dll.Select("1=1 ")[i]["Notes"].ToString();
                    sa.Phone = dll.Select("1=1 ")[i]["Cellolar"].ToString();
                    sa.IsAttention = GetOnlyUser(dll.Select("1=1 ")[i]["Cellolar"].ToString());
                    cn.rows.Add(sa);
                }
                cn.total =Convert.ToInt32(dll.Select("1=1 ")[0]["sum"]);
            }
            else {
                
            }
            return MyUtility.GetJsonFromObj(cn);
        }
        private static string GetOnlyUser(string id) {
            string ls="";
            string accss = MyUtility.GetGroupAccess();
            RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/user/get?access_token="+accss+"&userid="+id);
            HttpWebResponse rep = req.GetResponse();
            string a = req.GetResponseContent(rep);
            dynamic dy = MyUtility.GetObjFromJson<dynamic>(a);
            if (dy.errcode == 0)
            {
                ls=Convert.ToString(dy.status);
            }
            else {
                ls="3";
            }
            //List<Customer> userlist = new List<Customer>();
            //Customer mm = new Customer();
            //mm.status = dy.userlist[i].status;
            //userlist.Add(mm);

            return ls;
        }
        private static List<Department> getTree()
        {
            string accss = MyUtility.GetGroupAccess();
            RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/department/list?access_token=" + accss);
            HttpWebResponse rep = req.GetResponse();
            string a = req.GetResponseContent(rep);
            string dy = Convert.ToString(MyUtility.GetObjFromJson<dynamic>(a).department);
            List<Department> department = new List<Department>();
            department = MyUtility.GetObjFromJson<List<Department>>(dy);
            //for (int i = 0; i <= dy.department.Count; i++) {

            //}
            return department;
        }
        private static string getUser(string id)
        {
            string accss = MyUtility.GetGroupAccess();
            RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/user/list?access_token=" + accss + "&department_id=" + id + "&fetch_child=1");
            HttpWebResponse rep = req.GetResponse();
            string a = req.GetResponseContent(rep);
            dynamic dy = MyUtility.GetObjFromJson<dynamic>(a);
            List<Customer> userlist = new List<Customer>();
            for (int i=0;i<dy.userlist.Count;i++) {
                Customer mm = new Customer();
                mm.userid=dy.userlist[i].userid;
                mm.status= dy.userlist[i].status;
                mm.name = dy.userlist[i].name;
                mm.mobile = dy.userlist[i].mobile;
                userlist.Add(mm);
            }
            return MyUtility.GetJsonFromObj(userlist);
        }
        private static DataSet ExcelToDS(string Path)
        {
            string strConn = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + Path + ";Extended Properties='Excel 12.0; HDR=NO; IMEX=1'";//此连接可以操作.xls与.xlsx文件

            //string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 8.0;";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;
            strExcel = "select * from [sheet1$]";//sheet1表示表名
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");
            conn.Close();
            return ds;
        }
        //private static bool Exist_Usr(string id) {
        //    string accss = MyUtility.GetGroupAccess();
        //    RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/user/get?access_token=" + accss + "&userid=" + id);
        //    HttpWebResponse rep = req.GetResponse();
        //    string a = req.GetResponseContent(rep);
        //    dynamic ss = MyUtility.GetObjFromJson<dynamic>(a);
        //    if (ss.errcode == 60111)
        //    {
        //        return false;
        //    }
        //    else if (ss.errcode == 0)
        //    {
        //        return true;
        //    }
        //    else {
        //        return false;
        //    }
        //}
        private static MessageApi Exist_Par(string str)
        {
            MessageApi mm = new MessageApi();
            string id = "";
            string[] ss = str.Split('/');
            List<Department> dep = getTree();
            for (int i = 0; i < ss.Length; i++)
            {
                if (i == 0)
                {
                    if (ss[0] != get_ParName("1", dep))
                    {
                        mm.code = "404";
                        mm.txt = "部门输入错误" + ss[0];
                        return mm;
                    }
                    else
                    {
                        id = "1";
                    }
                }
                else
                {
                    id = get_ParId(id, ss[i], dep);
                }
            }
            mm.code = "0";
            mm.txt = id;
            return mm;
        }
        private static MessageApi Create_Usr(string depId, string Name, string userId, string mobile) {
            MessageApi mm = new MessageApi();
            User us = new User();
            us.department = depId;
            us.name = Name;
            us.userid = userId;
            us.mobile = mobile;
            string data = MyUtility.GetJsonFromObj(us);
            string accss = MyUtility.GetGroupAccess();
            RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/user/create?access_token=" + accss);
            req.RequestObj.Method = "POST";
            req.RequestObj.ContentType = "application/Json";
            req.SetRequestContent(data);
            HttpWebResponse rep = req.GetResponse();
            string x = req.GetResponseContent(rep);
            dynamic dy = MyUtility.GetObjFromJson<dynamic>(x);
            if (dy.errcode == 0)
            {
                mm.code = "0";
                mm.txt = "";
                return mm;
            }
            else
            {
                mm.code = "404";
                mm.txt = dy.errmsg;
                return mm;
            }
        }
        private static bool Exist_Usr(string userid) {
            string accss = MyUtility.GetGroupAccess();
            RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/user/get?access_token=" + accss + "&userid=" + userid );
            HttpWebResponse rep = req.GetResponse();
            string a = req.GetResponseContent(rep);
            dynamic dy = MyUtility.GetObjFromJson<dynamic>(a);
            if (dy.errcode == 0)
            {
                return true;
            }
            else {
                return false;
            }
        }
        private static string Create_Par(string Name, string pid)
        {
            Department us = new Department();
            us.name = Name;
            us.parentid = pid;
            string data = MyUtility.GetJsonFromObj(us);
            string accss = MyUtility.GetGroupAccess();
            RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/department/create?access_token=" + accss);
            req.RequestObj.Method = "POST";
            req.RequestObj.ContentType = "application/Json";
            req.SetRequestContent(data);
            HttpWebResponse rep = req.GetResponse();
            string x = req.GetResponseContent(rep);
            dynamic dy = MyUtility.GetObjFromJson<dynamic>(x);
            if (dy.errcode == 0)
            {
                return dy.id;
            }
            else
            {
                return "false";
            }

        }
        private static string get_ParName(string id, List<Department> dep)
        {
            string str = "";
            for (int i = 0; i < dep.Count; i++)
            {
                if (dep[i].id == id)
                {
                    str = dep[i].name;
                    break;
                }
            }
            return str;
        }
        private static string get_ParId(string parentid, string Name, List<Department> dep)
        {
            for (int i = 0; i < dep.Count; i++)
            {
                if (dep[i].parentid == parentid && dep[i].name == Name)
                {
                    return dep[i].id;
                }
            }
            return Create_Par(Name, parentid);
        }
        private static MessageApi createUser(string depId, string Name, string userId, string mobile)
        {
            MessageApi mm = new MessageApi();
            User us = new User();
            Regex reg = new Regex(@"^[1]+\d{10}");
            if (depId == "" || Name == "" || userId == "")
            {
                mm.code = "404";
                mm.txt = "用户信息不完善";
                return mm;
            }
            else if (!reg.IsMatch(userId))
            {
                mm.code = "404";
                mm.txt = "Id要11位电话号码";
                return mm;
            }
            else
            {
                if (Exist_Usr(userId))
                {
                    us.department = depId;
                    us.name = Name;
                    us.userid = userId;
                    us.mobile = mobile;
                    string data = MyUtility.GetJsonFromObj(us);
                    string accss = MyUtility.GetGroupAccess();
                    RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/user/update?access_token=" + accss);
                    req.RequestObj.Method = "POST";
                    req.RequestObj.ContentType = "application/Json";
                    req.SetRequestContent(data);
                    HttpWebResponse rep = req.GetResponse();
                    string x = req.GetResponseContent(rep);
                    dynamic dy = MyUtility.GetObjFromJson<dynamic>(x);
                    if (dy.errcode == 0)
                    {
                        mm.code = "0";
                        mm.txt = "";
                        return mm;
                    }
                    else
                    {
                        mm.code = "404";
                        mm.txt = dy.errmsg;
                        return mm;
                    }
                }
                else {
                    mm = Create_Usr(depId,Name,userId,mobile);
                    return mm;
                }
            }
        }
        private static string createTag(string tagName)
        {
            Tag us = new Tag();
            us.tagname = tagName;
            string data = MyUtility.GetJsonFromObj(us);
            string accss = MyUtility.GetGroupAccess();
            RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/tag/create?access_token=" + accss);
            req.RequestObj.Method = "POST";
            req.RequestObj.ContentType = "application/Json";
            req.SetRequestContent(data);
            HttpWebResponse rep = req.GetResponse();
            string x = req.GetResponseContent(rep);
            dynamic dy = MyUtility.GetObjFromJson<dynamic>(x);
            if (dy.errcode == 0)
            {
                return dy.tagid;
            }
            else
            {
                return "";
            }

        }
        private static List<Tag> get_TagList()
        {
            string accss = MyUtility.GetGroupAccess();
            RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/tag/list?access_token=" + accss);
            HttpWebResponse rep = req.GetResponse();
            string a = req.GetResponseContent(rep);
            List<Tag> tt = new List<Tag>();
            string json = Convert.ToString(MyUtility.GetObjFromJson<dynamic>(a).taglist);
            tt = MyUtility.GetObjFromJson<List<Tag>>(json);
            return tt;
        }
        private static void Exist_Tag(string str, string UserId)
        {
            MessageApi mm = new MessageApi();
            string id = "";
            string[] ss = str.Split('/');
            List<Tag> ll = get_TagList();
            for (int i = 0; i < ss.Length; i++)
            {
                id = get_TagId(ss[i], ll);
                createUser_tag(id, UserId);
            }
        }
        private static string get_TagId(string TagName, List<Tag> tt)
        {
            for (int i = 0; i < tt.Count; i++)
            {
                if (tt[i].tagname == TagName)
                {
                    return tt[i].tagid;
                }
            }
            return createTag(TagName);
        }
        private static void createUser_tag(string tagId, string UserId)
        {
            UserTag us = new UserTag();
            us.tagid = tagId;
            us.userlist.Add(UserId);
            string data = MyUtility.GetJsonFromObj(us);
            string accss = MyUtility.GetGroupAccess();
            RequestContainer req = new RequestContainer("https://qyapi.weixin.qq.com/cgi-bin/tag/addtagusers?access_token=" + accss);
            req.RequestObj.Method = "POST";
            req.RequestObj.ContentType = "application/Json";
            req.SetRequestContent(data);
            HttpWebResponse rep = req.GetResponse();
            string x = req.GetResponseContent(rep);
        }
        /// <summary>
        /// 成为开发者的第一步，验证并相应服务器的数据
        /// </summary>
        private static string Auth(string msg_signature, string timestamp, string nonce, string echostr)
        {

            string token = "KjGqfM19A";//从配置文件获取Token
            string encodingAESKey = "hLNLl6HhKeh48WIicxDs3GbEZGUSoT9JUCtL7vyG5bL";//从配置文件获取EncodingAESKey
            string corpId = "wx811b855e73c9b606";//从配置文件获取corpId
            string echoString = echostr;
            string signature = msg_signature;//企业号的 msg_signature
            string decryptEchoString = "";
            if (new CorpBasicApi().CheckSignature(token, signature, timestamp, nonce, corpId, encodingAESKey, echoString, ref decryptEchoString))
            {
                if (!string.IsNullOrEmpty(decryptEchoString))
                {
                    return decryptEchoString;
                }
            }
            return "";
        }
        private static void Write(string path, string str)
        {
            FileStream fs = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            sw.WriteLine(str);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }
        #endregion
    }
}