﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISeeMonitor.Models
{
    public class searchalljob
    {
        public string owner_id { get; set; }
        public string fname { get; set; }
        public string job_id { get; set; }
        public string job_status_code { get; set; }
        public string limit { get; set; }
    }
}
