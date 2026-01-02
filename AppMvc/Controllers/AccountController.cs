using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using System.Diagnostics;

using AppMvc.Models;
using AppMvc.SeidoHelpers;
using Encryption;
using Models.Authorization;
using Services;
using Models.DTO;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppMvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<User> _userManager;
        private IUserStore<User> _userStore;
        private SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IUserEmailStore<User> _emailStore;
        private Encryptions _encryptions;
        private readonly ILoginService _wapiLoginService;

        private record ConfirmationToken(string userId, string token);

        public AccountController(ILogger<AccountController> logger, UserManager<User> userManager,
                                  IUserStore<User> userStore, SignInManager<User> signInManager,
                                  IEmailSender emailSender, Encryptions encryptions,
                                  ILoginService wapiLoginService)
        {
            _logger = logger;
            _userManager = userManager;
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _encryptions = encryptions;
            _wapiLoginService = wapiLoginService;

            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            _emailStore = (IUserEmailStore<User>)_userStore;

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel vm, string code)
        {
            var confirmToken = _encryptions.AesDecryptFromBase64<ConfirmationToken>(code);

            var user = await _userManager.FindByIdAsync(confirmToken.userId);
            var result = await _userManager.ConfirmEmailAsync(user, confirmToken.token);

            if (result.Succeeded)
            {
                return View("ConfirmEmail", vm);
            }
            else
            {
                //Here I simple use Validation Error to show error from Identity.
                //Could be a seperate modal or separate page
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                vm.ValidationResult = new ModelValidationResult(true, identityErrors, null);

                //Send another email
                var confirmTokenNew = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var codeNew = _encryptions.AesEncryptToBase64(new ConfirmationToken(confirmToken.userId, confirmTokenNew));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { code = codeNew },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return View("ConfirmEmail", vm);
            }
        }

        [HttpGet]
        public IActionResult Register(RegisterViewModel vm)
        {
            vm.RegUser = new RegisterViewModel.UserIM();
            return View("Register", vm);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterSave(RegisterViewModel vm)
        {
            var returnUrl = Url.Content("~/");
            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult))
            {
                vm.ValidationResult = validationResult;
                return View("Register", vm);
            }

            //Continute to create a user with ASP.NET Core Identity
            var user = vm.RegUser.UpdateModel(new User());

            await _userStore.SetUserNameAsync(user, user.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, vm.RegUser.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var code = _encryptions.AesEncryptToBase64(new ConfirmationToken(userId, confirmToken));

                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return View("RegisterConfirmation", new { email = user.Email, returnUrl = returnUrl });
                }
                else
                {
                    _logger.LogInformation("User {Email} registered successfully", user.Email);

                    // Authenticate with WebAPI to obtain JWT token
                    var wapiResponse = await _wapiLoginService.LoginUserAsync(new LoginCredentialsDto
                    {
                        UserNameOrEmail = "dbo1",
                        Password = "dbo1"
                    });
                    _logger.LogInformation("WebAPI authentication succeeded for user {Email}", user.Email);

                    // Store the JWT token in the authentication properties
                    await JwtTokenStorage.StoreTokenAsync(
                        _signInManager,
                        user,
                        true,
                        wapiResponse.Item.JwtToken);
                    _logger.LogInformation("WebAPI token stored for user {Email}", user.Email);

                    return LocalRedirect(returnUrl);
                }
            }
            else
            {
                //Here I simple use Validation Error to show error from Identity.
                //Could be a seperate modal or separate page
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                vm.ValidationResult = new ModelValidationResult(true, identityErrors, null);
                return View("Register", vm);
            }
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View("RegisterConfirmation");
        }

        [HttpGet]
        public IActionResult Login(LoginViewModel vm)
        {
            vm.LoginCreds = new LoginViewModel.LoginIM();
            return View("Login", vm);
        }

        [HttpPost]
        public async Task<IActionResult> LoginOk(LoginViewModel vm)
        {
            var returnUrl = Url.Content("~/");

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult))
            {
                vm.ValidationResult = validationResult;
                return View("Login", vm);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(vm.LoginCreds.Email, vm.LoginCreds.Password, vm.LoginCreds.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Retrieve user
                var user = await _signInManager.UserManager.FindByEmailAsync(vm.LoginCreds.Email);
                _logger.LogInformation("User {Email} logged in successfully", user.Email);

                // Authenticate with WebAPI to obtain JWT token
                var wapiResponse = await _wapiLoginService.LoginUserAsync(new LoginCredentialsDto
                {
                    UserNameOrEmail = "dbo1",
                    Password = "dbo1"
                });
                _logger.LogInformation("WebAPI authentication succeeded for user {Email}", user.Email);

                // Store the JWT token in the authentication properties
                await JwtTokenStorage.StoreTokenAsync(
                    _signInManager,
                    user,
                    vm.LoginCreds.RememberMe,
                    wapiResponse.Item.JwtToken);
                _logger.LogInformation("WebAPI token stored for user {Email}", user.Email);                return LocalRedirect("/");
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = vm.LoginCreds.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }

            //Failed Login
            vm.ValidationResult = new ModelValidationResult(true, new List<string>() { "Invalid login attempt." }, null);
            return View("Login", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var returnUrl = Url.Content("~/");

            await _signInManager.SignOutAsync();
            return LocalRedirect(returnUrl);
        }
    }
}

