using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Management_platf.Models
{
    public class Content
    {
        public int total { get; set; }
        public List<Object> rows = new List<Object>();
    }
}