using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Data;
using System.Data.Odbc;

namespace WeiXinApi.Models
{
    public class SQLHelper
    {
        public  string connectionString { get; set; }
        public string CompanyDB { get; set; }  //数据库名字
        public string ServiceLayerUrl { get; set; }
        public static Company oCompany;
        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="DBName"></param>
        public SQLHelper( string DBName)
        {
            DecryptEncrypt decryptEncrypt = new DecryptEncrypt();
            string DbUserName = "SYSTEM";          
            string DbPassword = decryptEncrypt.DecryptString("ewkuUzieRFmtZ+8oOt7jlw==");
            string Password = decryptEncrypt.DecryptString("r9Vme0oG3tk=");
            CompanyDB = DBName;
            string server = "10.7.4.52:30015";
            if (DBName == "SBOCOMMON")  //如果是加载登录界面时获取帐套列表时，看配置文件里面的companyDB是否为空，不为空，则CompanyDB赋值，用于绑定帐套列表默认值
            {
                CompanyDB = "SBOCOMMON";
            }
            ServiceLayerUrl = "https://10.7.4.53:50000/b1s/v1/";
            if (Environment.Is64BitProcess)
            {
                connectionString = "Driver={HDBODBC};SERVERNODE={" + server + "};Database=" + DBName + ";uid=" + DbUserName + ";PWD=" + DbPassword + "";
            }
            else
            {
                connectionString = "Driver={HDBODBC32};SERVERNODE={" + server + "};Database=" + DBName + ";uid=" + DbUserName + ";PWD=" + DbPassword + "";
            }
        }
        /// <summary>
        /// 连接DI接口
        /// </summary>
        /// <param name="DBName"></param>
        /// <param name="UserName"></param>
        /// <param name="password"></param>
        public SQLHelper( string DBName, string UserName, string password)
        {
            oCompany = new Company();
            oCompany.language = SAPbobsCOM.BoSuppLangs.ln_English;
            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB;
            string server = "10.7.4.52:30015";
            CompanyDB = DBName;
            oCompany.Server = server;
            oCompany.UserName = UserName;
            oCompany.Password = password;
            oCompany.CompanyDB = DBName;
            oCompany.LicenseServer = "10.7.4.53:40000";
            int lRetCode = oCompany.Connect();
            string msg = oCompany.GetLastErrorDescription();
        }
        public SQLHelper(string TransDB, string DBName, string UserName, string password)
        {
            oCompany = new Company();
            oCompany.language = SAPbobsCOM.BoSuppLangs.ln_English;
            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB;
            DecryptEncrypt decryptEncrypt = new DecryptEncrypt();
            string server = "10.7.4.52:30015";
            oCompany.Server = server;
            string DbUserName = "SYSTEM";
            oCompany.UserName = UserName;
            string DbPassword = decryptEncrypt.DecryptString("ewkuUzieRFmtZ+8oOt7jlw==");
            oCompany.Password = password;
            string Password = decryptEncrypt.DecryptString("r9Vme0oG3tk=");
            CompanyDB = DBName;
            oCompany.CompanyDB = DBName;
            if (DBName == "SBOCOMMON")  //如果是加载登录界面时获取帐套列表时，看配置文件里面的companyDB是否为空，不为空，则CompanyDB赋值，用于绑定帐套列表默认值
            {
                CompanyDB = "SBOCOMMON";
            }
            ServiceLayerUrl = "https://10.7.4.53:50000/b1s/v1/";
            oCompany.LicenseServer = "10.7.4.53:40000";
            if (Environment.Is64BitProcess)
            {
                connectionString = "Driver={HDBODBC};SERVERNODE={" + server + "};Database=" + DBName + ";uid=" + DbUserName + ";PWD=" + DbPassword + "";
            }
            else
            {
                connectionString = "Driver={HDBODBC32};SERVERNODE={" + server + "};Database=" + DBName + ";uid=" + DbUserName + ";PWD=" + DbPassword + "";
            }
            int lRetCode = oCompany.Connect();
            string msg = oCompany.GetLastErrorDescription();
        }

        public string InitValue(object o)//
        {
            if (o != DBNull.Value && o != null)
            {
                return o.ToString().TrimEnd();
            }
            else
            {
                return "";
            }
        }
        public double InitDouble(object o)
        {
            if (o != DBNull.Value && o != null)
            {
                return Convert.ToDouble(o);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 动态给单据实体类赋值
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tmpdr"></param>
        /// <param name="placeStr">要替换的列值名</param>
        public void SetDocumentByDataRow(object model, System.Data.DataRow tmpdr, string placeStr, string DisCol)
        {
            for (int i = 0; i < tmpdr.Table.Columns.Count; i++)
            {
                try
                {
                    string colName = tmpdr.Table.Columns[i].ColumnName;
                    if (colName != "ID" && colName.StartsWith(placeStr) && DisCol.IndexOf(colName) < 0)
                        SetFieldValueInSource(model, colName.Substring(1, colName.Length - 1), tmpdr[i]);
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// source 原始对象   value 目标对象
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetFieldValueInSource(object source, string name, object value)
        {
            if (source != null && !string.IsNullOrEmpty(name))
            {
                string[] names = name.Split('.');
                object obj = source;
                PropertyInfo property = null;

                for (int i = 0; i < names.Length - 1; i++)
                {
                    property = obj.GetType().GetProperty(names[i]);

                    if (property.GetValue(obj, null) == null)
                    {
                        property.SetValue(obj, Activator.CreateInstance(property.PropertyType), null);
                    }

                    obj = property.GetValue(obj, null);
                }

                if (obj != null)
                {
                    property = obj.GetType().GetProperty(names[names.Length - 1]);
                    property.SetValue(obj, FormatValueByType(value, property.PropertyType), null);
                }
            }
        }
        public object FormatValueByType(object value, Type propertyType)
        {
            string text = value.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                string outType = (propertyType.IsGenericType) ? Nullable.GetUnderlyingType(propertyType).Name : propertyType.Name;
                Type type = typeof(Convert);
                MethodInfo method = type.GetMethod("To" + outType, new Type[] { typeof(string) });

                return method.Invoke(null, new object[] { text });
            }

            return null;
        }

        public System.Data.DataTable GetDataTable(string sql)
        {
            DataTable tmpdt;
            OdbcConnection connection = new OdbcConnection(connectionString);
            try
            {
                connection.Open();
                OdbcCommand command = new OdbcCommand();
                DataSet ds = new DataSet();
                command.Connection = connection;
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                new OdbcDataAdapter(command).Fill(ds);
                tmpdt = ds.Tables[0];
                connection.Close();
            }
            finally
            {
                connection.Dispose();
            }
            return tmpdt;

        }
        public void UpdateDataTable(string sql)
        {
            DataTable tmpdt;
            OdbcConnection connection = new OdbcConnection(connectionString);
            try
            {
                connection.Open();
                OdbcCommand command = new OdbcCommand();
                DataSet ds = new DataSet();
                command.Connection = connection;
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                new OdbcDataAdapter(command).Fill(ds);
                connection.Close();
            }
            finally
            {
                connection.Dispose();
            }

        }
    }
}