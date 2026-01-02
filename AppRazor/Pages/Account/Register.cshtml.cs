using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Models;
using Models.Authorization;
using AppRazor.SeidoHelpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Services;
using Models.DTO;
using Encryption;

namespace AppRazor.Pages
{
	public class RegisterModel : PageModel
    {
        private readonly ILogger<RegisterModel> _logger;

        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILoginService _wapiLoginService;

        [BindProperty]
        public UserIM RegUser { get; set; }

        //For Validation and Identity Errors
        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);

        public RegisterModel(
            ILogger<RegisterModel> logger,
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ILoginService wapiLoginService)
        {
            _logger = logger;
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _wapiLoginService = wapiLoginService;

            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            _emailStore = (IUserEmailStore<User>)_userStore;
        }

        public IActionResult OnGetAsync(string returnUrl = null)
        {
            RegUser = new UserIM();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var returnUrl = Url.Content("~/");
            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Continute to create a user with ASP.NET Core Identity
            var user = RegUser.UpdateModel(new User());

            await _userStore.SetUserNameAsync(user, user.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, RegUser.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId, code, returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = user.Email, returnUrl = returnUrl });
                }
                else
                {
                    _logger.LogInformation("User {Email} registered successfully", user.Email);

                    try
                    {
                        // Authenticate with WebAPI to obtain JWT token
                        var wapiResponse = await _wapiLoginService.LoginUserAsync(new LoginCredentialsDto
                        {
                            UserNameOrEmail = "dbo1",
                            Password = "dbo1"
                        });

                        if (wapiResponse?.Item?.JwtToken is null)
                        {
                            ValidationResult = new ModelValidationResult(true, new List<string>()
                            {
                                "Registered successfully, but could not obtain JWT token from Friends WebAPI."
                            }, null);
                            return Page();
                        }

                        // Store the JWT token in the authentication properties
                        await JwtTokenStorage.StoreTokenAsync(
                            _signInManager,
                            user,
                            true,
                            wapiResponse.Item.JwtToken);

                        _logger.LogInformation("WebAPI token stored for user {Email}", user.Email);
                        return LocalRedirect(returnUrl);
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogError(ex, "Friends WebAPI login call failed during registration");
                        ValidationResult = new ModelValidationResult(true, new List<string>()
                        {
                            "Registered successfully, but could not reach Friends WebAPI (DNS/network)."
                        }, null);
                        return Page();
                    }
                }
            }
            else
            {
                //Here I simple use Validation Error to show error from Identity.
                //Could be a seperate modal or separate page
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                ValidationResult = new ModelValidationResult(true, identityErrors, null);
                return Page();
            }
        }

        #region reguired by ASP.NET Core Identity
        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
        #endregion

        #region Input Model
        //InputModel (IM) is locally declared classes that contains ONLY the properties of the Model
        //that are bound to the <form> tag
        //EVERY property must be bound to an <input> tag in the <form>
        //These classes are in center of ModelBinding and Validation
        public class UserIM
        {
            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
//            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must be 8 characters long and contain at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            //The Basic IM methods
            public User UpdateModel(User model)
            {
                model.FirstName = this.FirstName;
                model.LastName = this.LastName;
                model.Email = this.Email;
                return model;
            }

            public UserIM() { }
            public UserIM(UserIM original)
            {
                FirstName = original.FirstName;
                LastName = original.LastName;
                Email = original.Email;
            }
            public UserIM(User model)
            {
                FirstName = model.FirstName;
                LastName = model.LastName;
                Email = model.Email;
            }
        }
        #endregion
    }
}
