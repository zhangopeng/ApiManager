using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Management_platf.Models
{
    public class UserTag
    {
        public string tagid { get; set; }
        public List<string> userlist = new List<string>();
    }
}