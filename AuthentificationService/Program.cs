using AuthentificationSerice.Core.Interfaces.Repositories;
using AuthentificationService.DAL;
using AuthentificationService.DAL.Models;
using AuthentificationService.DAL.Repositories;
using AuthentificationService.Service.Interfaces;
using AuthentificationService.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AuthDbContext>(options =>//ici tu ajoute "AuthDbcontext a ta boite de service
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
/*
 * Option.UseNpgsql indique que tu utilise le provider PostgrestSQL(via le package Npgsql.EntityFrameworkCore.PostgreSQL).
 * builder.Configuration.GetConnectionString("DefaultConnection")va chercher la chaine de connexion definie dans ton fichier appsetting.json
 
 */

//add the repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

//add the services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();

//coniguration JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)//active le systeme d'authentification base sur jw bearer tokens, cela veut que ton API attendra un token jwt dans l'en-tete Authorization: bearer <token>
    .AddJwtBearer(options =>// configure le middleware jwt pour savoir comment valider les tokens
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException());

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });





// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Pour que swagger puisse gérer l'authentification JWT, on ajoute une configuration spécifique pour SwaggerGen
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Authentication Service API",
        Version = "v1",
        Description = "Service d'authentification pour l'application de gestion de factures"
    });

    // Configuration pour ajouter le bouton "Authorize" dans Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrez 'Bearer' suivi d'un espace et du token JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed data (optionnel)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    context.Database.EnsureCreated(); // Crée la DB si elle n'existe pas
}

app.Run();