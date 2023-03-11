using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkManager.ViewModels
{
    #region registrazione
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "La password deve avere almeno 8 caratteri", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Conferma password")]
        [Compare("Password", ErrorMessage = "Le password non corrispondono")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Ruolo")]
        public string Ruolo { get; set; }
    }
    #endregion


    #region login
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "La password deve avere almeno 8 caratteri", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

    }
    #endregion

    #region recupero password
    public class LostPasswordViewModel
    {
        [Required]
        public string Username { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "La password deve avere almeno 8 caratteri", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Conferma password")]
        [Compare("Password", ErrorMessage = "Le password non corrispondono")]
        public string ConfirmPassword { get; set; }
    }
    #endregion
}
