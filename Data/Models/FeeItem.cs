using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models
{
    public class FeeItem
    {
        public int ResourceID { get; set; }
        public int ChargeTypeID { get; set; }
        public string FeeItemName { get; set; }
        public string FeeCategoryName { get; set; }
        public int FeeItemOrder { get; set; }
        public int FeeCategoryOrder { get; set; }
    }
}