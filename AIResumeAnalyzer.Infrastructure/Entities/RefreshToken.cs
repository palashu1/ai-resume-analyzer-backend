using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Infrastructure.Entities
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName ="nvarchar(500)")]
        public string TokenHash { get; set; } = string.Empty;
        [Column(TypeName = "nvarchar(1000)")]
        public string UserAgent { get; set; } = string.Empty;
        [Column(TypeName = "nvarchar(500)")]
        public string IpAddress { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public DateTime ExpiresOn { get; set; }

        public DateTime? RevokedOn { get; set; }
        public bool isActive { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
