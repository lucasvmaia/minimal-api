using minimal_api.DTOs;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapPost("/login", (LoginDTO loginDTO) => {
    if(loginDTO.Email == "adm@teste.com.br" && loginDTO.Senha == "12345")
        return Results.Ok("Login com sucesso!");
    else
        return Results.Unauthorized();
});

app.Run();