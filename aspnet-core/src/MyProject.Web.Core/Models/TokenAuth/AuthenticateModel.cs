 // This file is not generated, but this comment is necessary to exclude it from StyleCop analysis 
 // <auto-generated/> 
using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;

namespace MyProject.Models.TokenAuth
{
    public class AuthenticateModel
    {
        [Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string UserNameOrEmailAddress { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        public string Password { get; set; }
        
        public bool RememberClient { get; set; }
    }

    public class AuthenticateSSOModel
    {
        [Required]
        public string MSOUserId { get; set; }
    }
}
