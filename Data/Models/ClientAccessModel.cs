using LNF.PhysicalAccess;
using System;
using System.Collections.Generic;
using System.Web;

namespace Data.Models
{
    public class ClientAccessModel : ClientBaseModel
    {
        public string ActiveTab { get; set; }
        public DateTime? ExpireCutoff { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public IEnumerable<Badge> GetBadges()
        {
            return Provider.PhysicalAccess.GetBadge();
        }

        public IEnumerable<Card> GetCards()
        {
            if (ExpireCutoff.HasValue)
                return Provider.PhysicalAccess.GetExpiringCards(ExpireCutoff.Value);
            else
                return Provider.PhysicalAccess.GetCards();
        }

        public IEnumerable<Event> GetEvents()
        {
            return Provider.PhysicalAccess.GetEvents(StartDate.Value, EndDate.Value.AddDays(1));
        }

        public string DaysSinceLastAccess(Card c)
        {
            if (c.LastAccess.HasValue)
                return (DateTime.Now - c.LastAccess.Value).TotalDays.ToString("#0.00");
            else
                return string.Empty;
        }

        public IHtmlString FormatDateTime(DateTime? d, string defval = "")
        {
            if (!d.HasValue) return new HtmlString(defval);
            return new HtmlString(d.Value.ToString("M/d/yyyy<br />h:mm:ss tt"));
        }
    }
}