using Backend.Infrastructure.Integracoes.Nager;
using Backend.Infrastructure.Persistencia;
using Backend.Servicos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ClinicaDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("ConexaoPadrao");
    options.UseSqlServer(cs);
});

// HttpClient tipado para integração com Nager.Date (feriados públicos BR)
builder.Services.AddHttpClient<ClienteNager>(http =>
{
    http.BaseAddress = new Uri("https://date.nager.at");
    http.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddScoped<ServicoFeriados>();
builder.Services.AddScoped<ServicoDisponibilidade>();
builder.Services.AddScoped<ServicoAgendamentos>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Servimos o frontend (wwwroot) pelo próprio backend para simplificar o deploy
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
