using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models
{
    public class ResourceState
    {
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
        public string BuildingName { get; set; }
        public string LabName { get; set; }
        public string ProcessTechName { get; set; }
        public int PointID { get; set; }
        public string InterlockStatus { get; set; }
        public bool InterlockState { get; set; }
        public bool InterlockError { get; set; }
        public bool IsInterlocked { get; set; }
    }
}