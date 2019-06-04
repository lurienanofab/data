using LNF;
using LNF.Models.Data;
using LNF.Models.PhysicalAccess;
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
            return ServiceProvider.Current.PhysicalAccess.GetBadge();
        }

        public IEnumerable<Card> GetCards()
        {
            if (ExpireCutoff.HasValue)
                return ServiceProvider.Current.PhysicalAccess.GetExpiringCards(ExpireCutoff.Value);
            else
                return ServiceProvider.Current.PhysicalAccess.GetCards();
        }

        public IEnumerable<Event> GetEvents()
        {
            return ServiceProvider.Current.PhysicalAccess.GetEvents(StartDate.Value, EndDate.Value.AddDays(1));
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