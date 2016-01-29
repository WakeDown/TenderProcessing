﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeCalcDataAccessLayer.Models
{
    //Класс - юзер с ролью снабженец
    public class ProductManager : UserBase
    {
        public int PositionsCount { get; set; }

        public int CalculatesCount { get; set; }

        public int CalculatePositionsCount { get; set; }

    }
}
