﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Enums;

namespace SpeCalcDataAccessLayer.Models
{
    //класс - заявка
    public class TenderClaim
    {
        public int Id { get; set; }

        public string TenderNumber { get; set; }

        public DateTime TenderStart { get; set; }

        public DateTime ClaimDeadline { get; set; }

        public DateTime KPDeadline { get; set; }

        public string TenderStartString { get; set; }

        public string ClaimDeadlineString { get; set; }

        public string KPDeadlineString { get; set; }

        public string Comment { get; set; }

        public string Customer { get; set; }

        public string CustomerInn { get; set; }

        public double Sum { get; set; }

        public int SumCurrency { get; set; }

        public int DealType { get; set; }

        public string TenderUrl { get; set; }

        public int ClaimStatus { get; set; }

        public Manager Manager { get; set; }

        public int TenderStatus { get; set; }

        public DateTime RecordDate { get; set; }

        public string RecordDateString { get; set; }

        public UserBase Author { get; set; }

        public bool Deleted { get; set; }

        public double CurrencyUsd { get; set; }

        public double CurrencyEur { get; set; }

        public DateTime? DeliveryDate { get; set; }
        public DateTime? DeliveryDateEnd { get; set; }

        public string DeliveryPlace { get; set; }

        public DateTime? AuctionDate { get; set; }

        public string DeliveryDateString { get; set; }
        public string DeliveryDateEndString { get; set; }

        public string AuctionDateString { get; set; }

        public int PositionsCount { get; set; }

        public int CalculatesCount { get; set; }

        public int CalculatePositionsCount { get; set; }

        public List<ProductManager> ProductManagers { get; set; }

        public List<SpecificationPosition> Positions { get; set; }

        public string StrSum { get; set; }

        public List<ClaimCert> Certs { get; set; }
        public List<TenderClaimFile> Files { get; set; } 
    }
}
