using System;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Entities
{
    public class DirectoryUpn : IHasLongId
    {
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }

        [ForeignKey(typeof(ApplicationRegistration))]
        public long ApplicationRegistrationId { get; set; }

        [Required]
        [Index(unique:true)]
        [StringLength(128)]
        public string Suffix { get; set; }

        [Required]
        public DateTimeOffset DateCreatedUtc { get; set; }

        public DateTimeOffset? DateOnlineUtc { get; set; }

        public DateTimeOffset? DateOfflineUtc { get; set; }
    }
}