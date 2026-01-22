global using FastEndpoints;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using MaoMao.API.Services.Contract;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using MaoMao.API.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var mongoClient = new MongoClient(builder.Configuration.GetValue<string>("Mongo:ConnectionString"));
var mongoDatabase = mongoClient.GetDatabase(builder.Configuration.GetValue<string>("Mongo:Database"));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.SwaggerDocument();
builder.Services.AddSwaggerGen(opt => // Set up auth in swagger w/ jwt
{
	opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Paste your JWT token here",
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		BearerFormat = "JWT",
		Scheme = "Bearer"
	});
	opt.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});
});
builder.Services.AddFastEndpoints();
builder.Services.AddAuthenticationJwtBearer(s => {
	s.SigningKey = builder.Configuration["JWT:SecretKey"]!;
}, b =>
{
	b.ClaimsIssuer = builder.Configuration["JWT:Issuer"];
	b.Audience = builder.Configuration["JWT:Audience"];
});
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IHashService, HashService>();
builder.Services.AddSingleton<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
	app.UseSwaggerGen();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseFastEndpoints();

app.Run();
