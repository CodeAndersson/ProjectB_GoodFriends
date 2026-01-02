using Microsoft.AspNetCore.Authorization;

using Services;
using Configuration.Extensions;
using DbContext.Extensions;
using DbRepos;
using Encryption.Extensions;
using Encryption;
using Models.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//adding support for several secret sources and database sources
//to use either user secrets or azure key vault depending on UseAzureKeyVault tag in appsettings.json
builder.Configuration.AddSecrets(builder.Environment);

//use encryption and multiple Database connections and their respective DbContexts
builder.Services.AddEncryptions(builder.Configuration);
builder.Services.AddDatabaseConnections(builder.Configuration);
builder.Services.AddUserBasedDbContext();

// adding version and environment info
builder.Services.AddVersionInfo();
builder.Services.AddEnvironmentInfo();

//Add IdentityServices to DbContext.MainDbContext
builder.Services.AddDefaultIdentity<User>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<DbContext.MainDbContext>();

// Configure cookie authentication to redirect to AccountController Login action
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow both HTTP and HTTPS in development
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "GoodFriendsAuth";

    // Enable saving tokens in the cookie - required for httpContext.GetTokenAsync to work
    options.Events.OnSigningIn = context =>
    {
        context.Properties.IsPersistent = true;
        return Task.CompletedTask;
    };
});

// Authorization handlers (resource-based)
builder.Services.AddSingleton<IAuthorizationHandler, FriendAuthorizationHandler>();

// Data source selector (WebApi vs local in-memory)
builder.Services.AddSingleton<IDataSourceActive, DataSourceActive>();

#region Injecting a dependency service to read FriendsWebApi
builder.Services.AddTransient<JwtTokenHandler>();
var webApiBaseUri = builder.Configuration["DataService:WebApiBaseUri"];

builder.Services.AddHttpClient(name: "FriendsWebApi", configureClient: options =>
{
    options.BaseAddress = new Uri(webApiBaseUri);
    options.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
            mediaType: "application/json",
            quality: 1.0));
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient(name: "GuestWebApi", configureClient: options =>
{
    options.BaseAddress = new Uri(webApiBaseUri);
    options.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
            mediaType: "application/json",
            quality: 1.0));
});

//Used for Identity email verification
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailService>();

//Inject Services
builder.Services.AddScoped<ILoginService, LoginServiceWapi>();

// Local in-memory datasource services
builder.Services.AddSingleton<Services.InMemory.InMemoryFriendsStore>();
builder.Services.AddScoped<AdminServiceLocal>();
builder.Services.AddScoped<FriendsServiceLocal>();
builder.Services.AddScoped<AddressesServiceLocal>();
builder.Services.AddScoped<PetsServiceLocal>();
builder.Services.AddScoped<QuotesServiceLocal>();

// WebApi datasource services (concrete types)
builder.Services.AddScoped<AdminServiceWapi>();
builder.Services.AddScoped<FriendsServiceWapi>();
builder.Services.AddScoped<AddressesServiceWapi>();
builder.Services.AddScoped<PetsServiceWapi>();
builder.Services.AddScoped<QuotesServiceWapi>();

// Active switchers (selected via SelectDataSource)
builder.Services.AddScoped<IAdminService, AdminServiceActive>();
builder.Services.AddScoped<IFriendsService, FriendsServiceActive>();
builder.Services.AddScoped<IAddressesService, AddressesServiceActive>();
builder.Services.AddScoped<IPetsService, PetsServiceActive>();
builder.Services.AddScoped<IQuotesService, QuotesServiceActive>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //https://en.wikipedia.org/wiki/HTTP_Strict_Transport_Security
    //https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl
    app.UseHsts();
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


//Mapped Get response example
app.MapGet("/hello", () =>
{
    //read the environment variable ASPNETCORE_ENVIRONMENT
    //Change in launchSettings.json, (not VS2022 Debug/Release)
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var envMyOwn = Environment.GetEnvironmentVariable("MyOwn");

    return $"Hello World!\nASPNETCORE_ENVIRONMENT: {env}\nMyOwn: {envMyOwn}";
});

app.Run();

