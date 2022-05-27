using Helpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("default");

builder.Services.AddDbContext<Database>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});
builder.Services.AddLogging(ops => {ops.AddConsole();});
builder.Services.AddCors();

Globals.secret = builder.Configuration.GetValue<string>("secret");

var app = builder.Build();

// Configure the HTTP request pipeline.
using (var scope = app.Services.CreateScope()) {
    scope.ServiceProvider.GetRequiredService<Database>().Database.Migrate();
} 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(ops => {
    ops.AllowAnyHeader();
    ops.AllowAnyMethod();
    ops.AllowAnyOrigin();
});

app.UseMiddleware<AuthenticationMiddleware>();

app.MapControllers();

app.Run();