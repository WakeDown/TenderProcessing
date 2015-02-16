﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Models
{
    public static class UserHelper
    {
        public static List<ProductManager> GetProductManagers()
        {
            return new List<ProductManager>()
            {
               new ProductManager(){Id = "dfsadfs", Name = "Гена"},
               new ProductManager(){Id = "fdbfgbv", Name = "Вася"},
               new ProductManager(){Id = "dfsdfvfdhadfs", Name = "Петр"},
               new ProductManager(){Id = "dfsdwqedqwefefadfs", Name = "Олег"},
               new ProductManager(){Id = "df45gfdgsadfs", Name = "Дима"},
               new ProductManager(){Id = "dfsvdfgdfgdfbadfs", Name = "Alex"},
               new ProductManager(){Id = "khnhbfgbdf", Name = "Stan"}
            };
        }

        public static ProductManager GetProductManagerFromActiveDirectoryByName(string name)
        {
            var managers = GetProductManagers();
            return managers.FirstOrDefault(x => x.Name == name);
        }

        public static ProductManager GetProductManagerFromActiveDirectoryById(string id)
        {
            var managers = GetProductManagers();
            return managers.FirstOrDefault(x => x.Id == id);
        }

        public static List<Manager> GetManagers()
        {
            return new List<Manager>
            {
                new Manager() {Id = "asd", Name = "Олег Иванов", SubDivision = "Barcelona", Chief = "Александров А.А."},
                new Manager() {Id = "rtre", Name = "Андрей Петров", SubDivision = "Borussia", Chief = "Широков Р.В."},
                new Manager() {Id = "fgdsf", Name = "Дмитрий Степанов", SubDivision = "Zenit", Chief = "Файзулин В.Г."}
            };
        }

        public static Manager GetManagerFromActiveDirectoryById(string id)
        {
            var managers = GetManagers();
            return managers.FirstOrDefault(x => x.Id == id);
        }
    }
}