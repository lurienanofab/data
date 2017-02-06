using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LNF.Repository;
using LNF.Repository.Data;
using Data.Models;

namespace Data.Controllers
{
    public class ApiNewsController : ApiController
    {
        public NewsItem[] Get(string option = "")
        {
            IEnumerable<News> query = DA.Current
                .Query<News>()
                .Where(x =>
                    x.NewsActive
                    && !x.NewsDeleted
                    && (x.NewsPublishDate == null || x.NewsPublishDate.Value < DateTime.Now)
                    && (x.NewsExpirationDate == null || x.NewsExpirationDate.Value > DateTime.Now))
                .OrderBy(x => x.NewsSortOrder);

            if (query.Count() == 0)
                query = DA.Current.Query<News>().Where(x => x.NewsDefault);

            if (option == "ticker")
                return query.Where(x => x.NewsTicker).Select(CreateNewsItem).ToArray();
            else
                return query.Where(x => !x.NewsTicker).Select(CreateNewsItem).ToArray();
        }

        private NewsItem CreateNewsItem(News news)
        {
            return new NewsItem()
            {
                NewsID = news.NewsID,
                CreatedByClientID = news.NewsCreatedByClient.ClientID,
                UpdatedByClientID = news.NewsUpdatedByClientID,
                ImageFileName = news.NewsImageFileName,
                ImageContentType = news.NewsImageContentType,
                Title = news.NewsTitle,
                Description = news.NewsDescription,
                CreatedDate = news.NewsCreatedDate,
                LastUpdate = news.NewsLastUpdate,
                PublishDate = news.NewsPublishDate,
                ExpirationDate = news.NewsExpirationDate,
                SortOrder = news.NewsSortOrder,
                Ticker = news.NewsTicker,
                Default = news.NewsDefault,
                Active = news.NewsActive,
                Deleted = news.NewsDeleted
            };
        }
    }
}
