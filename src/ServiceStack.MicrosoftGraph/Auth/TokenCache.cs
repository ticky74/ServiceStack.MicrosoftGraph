using System;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;

namespace ServiceStack.Azure.Auth
{
    public class TokenCache : IHasLongId
    {
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }      
        
        [Index(unique:true)]
        [Required]
        public int UserAuthId { get; set; }  

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