using APICatalogo.Context;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Extensions;
using APICatalogo.Filters;
using APICatalogo.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Catalogo", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Header de autorização JWT usando o esquema Bearer.\r\n\r\nInforme'Bearer'[espaço] e o seu token.\r\n\r\nExemplo:\'Bearer 12345abcdef\'",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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


string? mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(mySqlConnection,ServerVersion.AutoDetect(mySqlConnection)));

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


// JWT
// adiciona o manipulador de autenticação e define o esquema de autenticação utilizado: Bearer
// valida o emissor, a audiencia e a chave
// usando a chave secreta valida a assinatura
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidAudience = builder.Configuration["TokenConfiguration:Audience"],
            ValidIssuer = builder.Configuration["TokenConfiguration:Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:key"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                else if (context.Exception.GetType() == typeof(SecurityTokenInvalidSignatureException))
                {
                    context.Response.Headers.Add("Invalid-Token-Signature", "true");
                }
                // Adicione mais tratamentos de exceção conforme necessário

                return Task.CompletedTask;
            }
        };
    });





builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ApiLoggingFilter>();
var mappingConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

builder.Services.AddCors(op =>
{
    op.AddPolicy("PermitirApiRequest", b => b.WithOrigins("http://localhost:8081/").WithMethods("GET"));
});

IMapper mapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// pode ser limitado por verbos http (get, post, put, delete) utilizanod withmethods
//app.UseCors(opt => opt.WithOrigins("http://localhost:8081/").WithMethods("GET"));


app.UseCors();
// adiciona o middleware de tratamento de erros
app.ConfigureExceptionHandler();

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
