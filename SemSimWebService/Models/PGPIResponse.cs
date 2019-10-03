using CsvHelper.Configuration.Attributes;
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

    public class PGPISingleResponse
    {
        public string Input { get; set; }

        public List<PGPIResponse> Responses { get; set; }
    }

    public class DescriptionInput
    {
        [Index(0)]
        public string Description { get; set; }
    }
}