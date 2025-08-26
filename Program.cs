using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SupportWeb.Services;
using SupportWeb.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración de localización
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure HttpClient for API calls
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure SignalR
builder.Services.AddSignalR();

// Register services (ApiService ya está registrado con AddHttpClient arriba)
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "your-super-secret-key-here-make-it-long-enough");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "SupportApi",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "SupportWeb",
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Sesión de 30 minutos
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Cambiar de Strict a Lax para mejor compatibilidad
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    // NO configurar Cookie.Expiration para que sea una cookie de sesión
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// Configuración de cultura por querystring (?culture=en)
var supportedCultures = new[] { "es", "en" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// MIDDLEWARES PERSONALIZADOS DESHABILITADOS PARA DEBUG
// app.UseCustomAuthentication();
// app.UseRoleAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Rutas específicas por rol
app.MapControllerRoute(
    name: "usuario",
    pattern: "Usuario/{action=Dashboard}/{id?}",
    defaults: new { controller = "Usuario" });

app.MapControllerRoute(
    name: "soporte",
    pattern: "Soporte/{action=Dashboard}/{id?}",
    defaults: new { controller = "Soporte" });

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "dashboard",
    pattern: "dashboard/{action=Index}/{id?}",
    defaults: new { controller = "Dashboard" });

app.MapControllerRoute(
    name: "auth",
    pattern: "auth/{action=Login}",
    defaults: new { controller = "Auth" });

app.Run();
