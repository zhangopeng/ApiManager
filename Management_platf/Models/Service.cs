using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeiXinApi.Models
{
    public class Service
    {
        public SQLHelper sqlhelper;
        public string companyDb;
        public string username;
        public string password;
        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <param name="companyDB"></param>
        public Service(string companyDB)
        {
            companyDb = companyDB;
            sqlhelper = new SQLHelper(companyDb);
        }
        /// <summary>
        /// DI接口
        /// </summary>
        /// <param name="companyDB"></param>
        /// <param name="userName"></param>
        /// <param name="Password"></param>
        public Service(string companyDB, string userName, string Password) {
            companyDb = companyDB;
            username = userName;
            password = Password;
            sqlhelper = new SQLHelper(companyDB,userName,Password);
        }
        public Service(string te, string companyDB, string userName, string Password)
        {
            companyDb = companyDB;
            username = userName;
            password = Password;
            sqlhelper = new SQLHelper("",companyDB, userName, Password);
        }
    }
}