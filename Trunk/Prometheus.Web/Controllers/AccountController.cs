using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Prometheus.Authorization;
using Prometheus.Authorization.Data;
using Prometheus.BL.Interfaces;
using Prometheus.Model.Models.AuthorizationModel;
using Prometheus.Web.Services;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly IEmailSender _emailSender;
        private readonly IUserProfileService _userProfile;
        private readonly IAdapterService _adapterService;

        public AccountController(IUserHandler user, IUserProfileService userProfile, IEmailSender emailSender, IAdapterService adapterService) : base(user)
        {
            _emailSender = emailSender;
            _userProfile = userProfile;
            _adapterService = adapterService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _user.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                
                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(Request.Query["ReturnUrl"].First());
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ModelState.AddModelError("", "Failed to login");
            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _user.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (model.CompanyName != null)
            {
                var userProfile = _userProfile.SaveUserProfile(model);
                if (ModelState.IsValid && userProfile.Status == Common.Enums.StatusEnum.Success)
                {
                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email, UserProfileId = userProfile.Value };
                    var result = await _user.UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var code = await _user.UserManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                        _adapterService.AdapterSeed(userProfile.Value);

                        //await _user.SignInAsync(user, isPersistent: false);
                        //return RedirectToAction("Index", "Home");
                        return RedirectToAction("CheckEmailNotification", "Account");
                    }
                    else
                    {
                        _userProfile.DeleteUserProfile(userProfile.Value);
                    }
                }
            }
            else
            {
                var userProfile = _userProfile.SaveUserProfile(model);

                if (ModelState["Email"].ValidationState == ModelValidationState.Valid &&
                    ModelState["Password"].ValidationState == ModelValidationState.Valid &&
                    ModelState["ConfirmPassword"].ValidationState == ModelValidationState.Valid &&
                    userProfile.Status == Common.Enums.StatusEnum.Success)
                {
                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email, UserProfileId = userProfile.Value };
                    var result = await _user.UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var code = await _user.UserManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                        _adapterService.AdapterSeed(userProfile.Value);

                        //await _user.SignInAsync(user, isPersistent: false);
                        //return RedirectToAction("Index", "Home");
                        return RedirectToAction("CheckEmailNotification", "Account");
                    }
                    else
                    {
                        var errorList = result.Errors.ToList();

                        ModelState.AddModelError("", errorList[0].Description);
                        _userProfile.DeleteUserProfile(userProfile.Value);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Failed to register");
                    _userProfile.DeleteUserProfile(userProfile.Value);
                }
                return View("Register");
            }
            
            ModelState.AddModelError("", "Failed to register");
            return View(model);
        }
                
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = await _user.UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _user.UserManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _user.UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _user.UserManager.IsEmailConfirmedAsync(user)))
                {
                    ModelState.AddModelError("", "Invalid email address.");
                    return View();
                }
                
                var code = await _user.UserManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return RedirectToAction("CheckEmailNotification", "Account");
            }

            
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CheckEmailNotification()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordModel { Code = code };
            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _user.UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email address.");
                return View();
            }
            var result = await _user.UserManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            ModelState.AddModelError("", "Failed to reset password.");
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

    }
}
