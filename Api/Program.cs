using Api.Extensions;
using Api.Middlewares;
using Application;
using Application.Configuration.DataRetention;
using Application.Configuration.Email;
using Application.Configuration.Pluggy;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddApiAuthorization();

builder.Services.AddAuthRateLimiting();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddLocalization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    c.OperationFilter<AcceptLanguageHeaderOperationFilter>();
});

var corsAllowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200", "http://localhost:8080"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins(corsAllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.Configure<PluggySettings>(builder.Configuration.GetSection("Pluggy"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<DataRetentionSettings>(builder.Configuration.GetSection("DataRetention"));

builder.Services.AddApplication();

var autoMigrate = builder.Configuration
    .GetValue<bool>("Database:AutoMigrate");

var app = builder.Build();

app.UseForwardedHeaders();

if (autoMigrate)
{
    await DatabaseInitializer.InitializeAsync(app.Services);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var supportedCultures = new[] { "pt-BR", "en-US", "fr-FR" };

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
    SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
};

localizationOptions.RequestCultureProviders.Insert(
    0,
    new AcceptLanguageHeaderRequestCultureProvider()
);

app.UseRequestLocalization(localizationOptions);

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAngular");

app.UseMiddleware<AccessHeaderMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().RequireAuthorization(AuthorizationExtensions.PublicAssetPolicy);
}

app.MapControllers();

app.MapFallbackToFile("index.html").RequireAuthorization(AuthorizationExtensions.PublicAssetPolicy);

app.Run();

