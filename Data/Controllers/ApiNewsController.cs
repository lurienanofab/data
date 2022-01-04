using Data.Controllers.Api;
using Data.Models;
using LNF;
using LNF.Impl.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Data.Controllers
{
    [Route("api/news/{option}")]
    public class ApiNewsController : DataApiController
    {
        public ApiNewsController(IProvider provider) : base(provider) { }

        public NewsItem[] Get(string option = "")
        {
            IEnumerable<News> query = DataSession
                .Query<News>()
                .Where(x =>
                    x.NewsActive
                    && !x.NewsDeleted
                    && (x.NewsPublishDate == null || x.NewsPublishDate.Value < DateTime.Now)
                    && (x.NewsExpirationDate == null || x.NewsExpirationDate.Value > DateTime.Now))
                .OrderBy(x => x.NewsSortOrder);

            if (query.Count() == 0)
                query = DataSession.Query<News>().Where(x => x.NewsDefault);

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
