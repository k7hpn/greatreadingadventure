﻿using System.ComponentModel.DataAnnotations;

namespace GRA.Domain.Model
{
    public class DirectEmailHistory : Abstract.BaseDomainEntity
    {
        [Required]
        public string BodyHtml { get; set; }

        [Required]
        public string BodyText { get; set; }

        [Required]
        [MaxLength(255)]
        public string FromEmailAddress { get; set; }

        [Required]
        [MaxLength(255)]
        public string FromName { get; set; }

        [MaxLength(255)]
        public string OverrideToEmailAddress { get; set; }

        [Required]
        public string SentResponse { get; set; }

        [Required]
        [MaxLength(255)]
        public string Subject { get; set; }

        [Required]
        [MaxLength(255)]
        public string ToEmailAddress { get; set; }

        [MaxLength(255)]
        public string ToName { get; set; }

        public User User { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
