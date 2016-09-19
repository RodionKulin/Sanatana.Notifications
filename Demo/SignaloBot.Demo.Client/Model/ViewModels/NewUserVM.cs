using SignaloBot.Demo.Client.App_Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SignaloBot.Demo.Client.Model
{
    public class NewUserVM
    {
        [Required(ErrorMessageResourceType = typeof(ViewContent), ErrorMessageResourceName = "NewUserVM_InvalidEmailError")]
        [EmailAddress(ErrorMessageResourceType = typeof(ViewContent), ErrorMessageResourceName = "NewUserVM_InvalidEmailError", ErrorMessage = null)]
        public string Email { get; set; }

        [Required]
        [Display(ResourceType = typeof(ViewContent), Name = "NewUserVM_IsEmailEnabled")]
        public bool IsEmailEnabled { get; set; }
    }
}