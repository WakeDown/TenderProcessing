﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderProcessingDataAccessLayer.Models
{
    //базовый класс для справочников
    public class ServerDirectBase
    {
        public int Id { get; set; }

        public string Value { get; set; }
    }
}
