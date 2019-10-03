using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SemSimWebService.Models
{
    public class PGPIResponse
    {
        public string TransactionTypeCode { get; set; }
        public string Description { get; set; }
        public double PercentMatch { get; set; }
    }
}