using LNF.Repository.Data;
using System.Collections.Generic;

namespace Data.Models
{
    public class SettingsModel
    {
        public IEnumerable<GlobalSettings> Settings { get; set; }
    }
}