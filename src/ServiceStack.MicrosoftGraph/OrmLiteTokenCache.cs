using System;
using System.Data;
using ServiceStack.OrmLite;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.MicrosoftGraph.ServiceModel.Entities;
using ServiceStack.MicrosoftGraph.ServiceModel.Interfaces;

namespace ServiceStack.Azure
{
    public class OrmLiteTokenCache : ITokenCache
    {
        private readonly IDbConnectionFactory _connectionFactory;

        #region Queries

        private readonly Func<IDbConnection, string, SqlExpression<UserAuthTokenCache>> _userTokenFilter =
            (db, s) => db.From<UserAuthTokenCache>()
                .Where(x => x.UserName == s);
        #endregion Queries

        public OrmLiteTokenCache(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public string GetAccessToken(string userName)
        {
            using (var db = _connectionFactory.OpenDbConnection())
            {
                var data = db.Single(_userTokenFilter(db, userName));
                return data?.AccessToken;
            }
        }

        public async Task<string> GetAccessTokenAsync(string userName)
        {
            using (var db = _connectionFactory.OpenDbConnection())
            {
                var data = await db.SingleAsync(_userTokenFilter(db, userName));
                return data?.AccessToken;
            }
        }

        public void ClearTokenCache(string userName)
        {
            using (var db = _connectionFactory.OpenDbConnection())
            {
                db.Delete(_userTokenFilter(db, userName));
            }
        }

        public async Task ClearTokenCacheAsync(string userName)
        {
            using (var db = _connectionFactory.OpenDbConnection())
            {
                await db.DeleteAsync(_userTokenFilter(db, userName));
            }
        }

        public void UpdateTokenCache(UserAuthTokenCache cacheItem)
        {
            if (cacheItem == null)
                throw new ArgumentNullException(nameof(cacheItem));

            if (string.IsNullOrWhiteSpace(cacheItem.AccessToken))
                throw new ArgumentNullException(nameof(cacheItem.AccessToken));

            if (string.IsNullOrWhiteSpace(cacheItem.RefreshToken))
                throw new ArgumentNullException(nameof(cacheItem.RefreshToken));

            using (var db = _connectionFactory.OpenDbConnection())
            {
                db.Delete(_userTokenFilter(db, cacheItem.UserName));
                db.Save(cacheItem);
            }
        }

        public async Task UpdateTokenCacheAsync(UserAuthTokenCache cacheItem)
        {
            if (cacheItem == null)
                throw new ArgumentNullException(nameof(cacheItem));

            if (string.IsNullOrWhiteSpace(cacheItem.AccessToken))
                throw new ArgumentNullException(nameof(cacheItem.AccessToken));

            if (string.IsNullOrWhiteSpace(cacheItem.RefreshToken))
                throw new ArgumentNullException(nameof(cacheItem.RefreshToken));

            using (var db = _connectionFactory.OpenDbConnection())
            {
                await db.DeleteAsync(_userTokenFilter(db, cacheItem.UserName));
                await db.SaveAsync(cacheItem);
            }
        }
    }
}