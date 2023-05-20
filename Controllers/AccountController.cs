using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleCRUD.Models;
using SimpleCRUD.Services.STMP;
using SimpleCRUD.View_Models;

namespace SimpleCRUD.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailSender _emailSender;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }


    [HttpGet]
    public async Task<IActionResult> Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = new MailAddress(model.Email).User
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            user = await _userManager.FindByEmailAsync(model.Email);
            //generate token to be send to user email
            var Token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            if (result.Succeeded)
            {
                //this link will be sent to user email
                var ConfirmationLink = Url.ActionLink("ConfirmEmail", "Account", new
                {
                    userId = user.Id, Token
                });
                //send it using emailsender service
                Console.WriteLine(ConfirmationLink);
                //await _emailSender.SendEmailAsync(user.Email,"Confirm your account","Please confirm your account by clicking this link: <a href=\"" +ConfirmationLink+ "\">link</a>");
                return RedirectToAction("Index", "Movies");
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ConfirmEmailCompleted()
    {
        return View();
    }

    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded) return RedirectToAction(nameof(ConfirmEmailCompleted));

        return new NotFoundResult();
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
            if (result.RequiresTwoFactor) return RedirectToAction("LoginWithMFA", "Account");
            if (result.Succeeded) RedirectToAction("index", "Movies");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOff()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(MoviesController.Index), "Movies");
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
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || await _userManager.IsEmailConfirmedAsync(user))
                return View("ForgotPasswordConfirmation");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callBackUrl = Url.Action("ResetPassword", "Account", new {userId = user.Id, code},
                HttpContext.Request.Scheme);
            //Print it in terminal instead of sending it to user email
            Console.WriteLine(callBackUrl);
            return View("ForgotPasswordConfirmation");
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string code)
    {
        var model = new ResetPasswordViewModel {Code = code};
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return RedirectToAction(nameof(ResetPasswordConfirmation), "Account");

        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded) return RedirectToAction(nameof(ResetPasswordConfirmation), "Account");

        return View();
    }

    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> TwoFactorAuthSetup()
    {
        var user = await _userManager.GetUserAsync(User);
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var token = await _userManager.GetAuthenticatorKeyAsync(user);
        var QrCodeUrl = $"otpauth://totp/aspnetidentity:{user.Email}?secret={token}&issuer=aspnetidentity&digits=6";
        var model = new TwoFactorAuthViewModel
        {
            Token = token,
            QRCodeUrl = QrCodeUrl
        };
        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> TwoFactorAuthSetup(TwoFactorAuthViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _userManager.VerifyTwoFactorTokenAsync(user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code);
            if (result)
                await _userManager.SetTwoFactorEnabledAsync(user, true);
            else
                ModelState.AddModelError("Verify", "Two Factor code is not valid");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult LoginWithMFA()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> LoginWithMFA(LoginWithMFAViewModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

        var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, false, false);

        var userId = await _userManager.GetUserIdAsync(user);

        if (result.Succeeded) return RedirectToAction("index", "Movies");

        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return View(model);
    }
}