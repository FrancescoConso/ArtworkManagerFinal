using ArtworkManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkManager.Controllers
{
    public class UsersController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UsersController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        #region Registrazione

        //restituisco la vista di registrazione
        public IActionResult Register()
        {
            return View();
        }

        //POST di un nuovo utente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(RegisterViewModel model)
        {
            //se tutti i dati sono stati compilati
            //effettuo la registrazione
            if (ModelState.IsValid)
            {
                try
                {
                    IdentityUser user = new IdentityUser { UserName = model.Username };
                    IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        //assegno ruolo all utente e effettuo il login
                        IdentityResult roleresult = await _userManager.AddToRoleAsync(user, model.Ruolo);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }           
            }

            return RedirectToAction(nameof(Register));
        }
        #endregion

        #region Login

        //restituisco la vista di login
        public IActionResult Login()
        {
            return View();
        }

        //POST di un form di login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginUser(LoginViewModel model)
        {
            //se tutti i dati sono stati compilati
            //effettuo la registrazione
            if (ModelState.IsValid)
            {
                try
                {
                    //se effettuo il login faccio il redirect alla homepage
                    Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }

            return RedirectToAction(nameof(Login));
        }
        #endregion

        #region Logout

        //operazione di logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Recupero Password

        //apro il form di inserimento username
        public IActionResult LostPassword()
        {
            return View();
        }

        //POST sul form di inserimento username
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmUsername(LostPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    IdentityUser result = await _userManager.FindByNameAsync(model.Username);
                    if (result != null)
                    {
                        //apro il form di reset password con l'id associato allo username
                        return RedirectToAction(nameof(ResetPassword), new { Id = result.Id });
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            return RedirectToAction(nameof(LostPassword));
        }

        //mostro il form di reset password
        public IActionResult ResetPassword(string Id)
        {
            return View();
        }


        //POST di reset password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordAction(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //recupero l'utente con id associato, se non lo trovo ritorno alla homepage
                    //in linea teorica entrando nella pagina ResetPassword dovrei avere sempre un id "buono"
                    IdentityUser user = await _userManager.FindByIdAsync(model.Id);
                    if (user != null)
                    {
                        //siccome mi viene richiesto un token che non ho lo creo sul momento
                        string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                        IdentityResult result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

                        if (result.Succeeded)
                        {
                            return RedirectToAction(nameof(ResetSuccessful));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                
            }
            return RedirectToAction("Index","Home");
        }

        //mostro la pagina di cambio password avvenuto
        public IActionResult ResetSuccessful()
        {
            return View();
        }

        #endregion

    }
}
