using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Management_platf.Models
{
    public class SapUser
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string Phone { get; set; }
        public string Note { get; set; }
        public string Tag { get; set; }
        public string IsAttention { get; set; }
    }
}