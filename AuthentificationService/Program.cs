using AuthentificationSerice.Core.Interfaces.Repositories;
using AuthentificationService.DAL;
using AuthentificationService.DAL.Models;
using AuthentificationService.DAL.Repositories;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AuthDbContext>(options =>//ici tu ajoute "AuthDbcontext a ta boite de service
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
/*
 * Option.UseNpgsql indique que tu utilise le provider PostgrestSQL(via le package Npgsql.EntityFrameworkCore.PostgreSQL).
 * builder.Configuration.GetConnectionString("DefaultConnection")va chercher la chaine de connexion definie dans ton fichier appsetting.json
 
 */

//add the services 
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
