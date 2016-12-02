using System;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Entities
{
    public class UserAuthTokenCache : IHasLongId
    {
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }      
        
        [Index(unique:true)]
        [Required]
        public string UserName { get; set; }  

        [Required]
        [StringLength(1024)]
        public string AccessToken { get; set; }

        public DateTimeOffset? AccessTokenExpiration { get; set; }

        [Required]
        [StringLength(1024)]
        public string RefreshToken { get; set; }

        public DateTimeOffset? RefreshTokenExpiration { get; set; }
    }
}