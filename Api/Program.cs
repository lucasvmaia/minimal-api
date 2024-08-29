using minimal_api.DTOs;
using minimal_api.Context;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Interface;
using minimal_api.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;

#region Builder
var builder = WebApplication.CreateBuilder(args);
var key = builder.Configuration.GetSection("Jwt").ToString();
if(string.IsNullOrEmpty(key)) key = "12345";

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option => {
    option.TokenValidationParameters = new TokenValidationParameters{
      ValidateLifetime = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
      ValidateIssuer = false,
      ValidateAudience = false
    };
});
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddDbContext<MyContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("conn"));
});
var app = builder.Build();
#endregion

#region Home
// app.MapGet("/", () => "Hello World!");
// app.MapGet("/", () => Results.Json(new Home()));
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administradores
string GerarTokenJwt(Administrador administrador)
{
    if(string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil),
    };
    var token = new JwtSecurityToken
    (
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );
    return new JwtSecurityTokenHandler().WriteToken(token);
}
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) => {
    //if(loginDTO.Email == "adm@teste.com.br" && loginDTO.Senha == "12345")
    var adm = administradorService.Login(loginDTO);
    // if(administradorService.Login(loginDTO) != null)
    if(adm != null)
    {
        string token = GerarTokenJwt(adm);
        // return Results.Ok("Login com sucesso!");
        return Results.Ok(new AdministradorLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService administradorService) => {
    // return Results.Ok(administradorService.Todos(pagina));
    var adms = new List<AdministradorModelView>();
    var administradores = administradorService.Todos(pagina);
    foreach(var adm in administradores)
    {
        adms.Add(new AdministradorModelView{
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorService administradorService) => {
    var administrador = administradorService.BuscarPorId(id);
    if(administrador == null) return Results.NotFound();
    // return Results.Ok(administrador);
    return Results.Ok(new AdministradorModelView{
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
        });
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorService administradorService) => {
    //if(loginDTO.Email == "adm@teste.com.br" && loginDTO.Senha == "12345")
    var mensagens = new ErrosDeValidacao{
        Mensagens = new List<string>()
    };
    
    if(string.IsNullOrEmpty(administradorDTO.Email))
        mensagens.Mensagens.Add("Email nao pode ser vazio");
    if(string.IsNullOrEmpty(administradorDTO.Senha))
        mensagens.Mensagens.Add("Senha nao pode ser vazia");
    if(administradorDTO.Perfil == null)
        mensagens.Mensagens.Add("Perfil nao pode ser vazio");
    if(mensagens.Mensagens.Count > 0)
        return Results.BadRequest(mensagens);
    
    var administrador = new Administrador{
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };    
     administradorService.Incluir(administrador);
     return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView{
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
        });
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");
#endregion

#region Veiculos
ErrosDeValidacao mensagensDTO(VeiculoDTO veiculoDTO)
{
    var mensagens = new ErrosDeValidacao{
        Mensagens = new List<string>()
    };


    if(string.IsNullOrEmpty(veiculoDTO.Nome))
        mensagens.Mensagens.Add("O nome nao pode ser vazio");
    
    if(string.IsNullOrEmpty(veiculoDTO.Marca))
        mensagens.Mensagens.Add("A marca nao pode ser vazio");

    if(veiculoDTO.Ano < 1950)
        mensagens.Mensagens.Add("Veiculo muito antigo, somente anos maiores que 1950");

    return mensagens;  
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoService veiculoService) => {
    var mensagens = mensagensDTO(veiculoDTO);
    if(mensagens.Mensagens.Count > 0)
        return Results.BadRequest(mensagens);
    
    var veiculo = new Veiculo{
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoService.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
.WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoService veiculoService) => {
    var veiculos = veiculoService.Todos(pagina);
    return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) => {
    var veiculo = veiculoService.BuscarPorId(id);

    if(veiculo == null) return Results.NotFound();

    return Results.Ok(veiculo);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
.WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoService veiculoService) => {
    var veiculo = veiculoService.BuscarPorId(id);
    if(veiculo == null) return Results.NotFound();
    
    var mensagens = mensagensDTO(veiculoDTO);
    if(mensagens.Mensagens.Count > 0)
        return Results.BadRequest(mensagens);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;
    veiculoService.Atualizar(veiculo);
    return Results.Ok(veiculo);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) => {
    var veiculo = veiculoService.BuscarPorId(id);
    if(veiculo == null) return Results.NotFound();

    veiculoService.Apagar(veiculo);
    return Results.NoContent();
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
#endregion