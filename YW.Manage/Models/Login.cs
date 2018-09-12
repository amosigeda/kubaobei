using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace YW.Manage.Models
{
    public class Login
    {
        [Required]
        [Display(Name = "服务器")]
        public string Server { get; set; }
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "登陆密码")]
        public string PassWord { get; set; }
    }
}