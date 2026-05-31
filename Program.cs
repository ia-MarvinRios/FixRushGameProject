using FixRushGameAPI.Data;
using FixRushGameAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//    c.SwaggerDoc("v1", new() { Title = "CarGame API", Version = "v1" }));



// Inyeccion de dependencias
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();

// CORS para Unity
builder.Services.AddCors(o => o.AddPolicy("Unity",
    p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//    app.UseSwaggerGen();
//}

app.UseCors("Unity");
app.UseAuthorization();
app.MapControllers();
app.Run();