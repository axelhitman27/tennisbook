using Microsoft.EntityFrameworkCore;
using Tennisbook.API.Data;
using Tennisbook.API.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Tennisbook API", Version = "v1" });
});

builder.Services.AddDbContext<TennisbookDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<TrainingService>();
builder.Services.AddScoped<TournamentService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddSingleton<QrCodeService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TennisbookDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
