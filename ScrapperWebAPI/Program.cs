using System.Runtime.InteropServices;
using ZaraScraperApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Console.BackgroundColor = ConsoleColor.DarkBlue;
    Console.ForegroundColor = ConsoleColor.White;
    Console.Clear();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n\n");
    Console.WriteLine("    ██████╗ ██████╗ ███████╗██████╗  ██████╗  ██████╗ ");
    Console.WriteLine("    ██╔══██╗██╔══██╗██╔════╝██╔══██╗██╔═══██╗██╔═══██╗");
    Console.WriteLine("    ██████╔╝██████╔╝█████╗  ██║  ██║██║   ██║██║   ██║");
    Console.WriteLine("    ██╔══██╗██╔══██╗██╔══╝  ██║  ██║██║   ██║██║   ██║");
    Console.WriteLine("    ██████╔╝██║  ██║███████╗██████╔╝╚██████╔╝╚██████╔╝");
    Console.WriteLine("    ╚═════╝ ╚═╝  ╚═╝╚══════╝╚═════╝  ╚═════╝  ╚═════╝ ");

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n           ✨ Welcome to Bredoo API ✨\n");

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"    ⏰ {DateTime.Now:HH:mm:ss} | 🚀 Starting application...\n");

    Console.ResetColor();
}
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // <-- Zara üçün lazım idi
builder.Logging.AddConsole();

//  CORS policy əlavə et
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // bütün domenlər
              .AllowAnyMethod()   // GET, POST, PUT, DELETE hamısına icazə
              .AllowAnyHeader();  // bütün header-lərə icazə
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<ZaraController>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("DefaultClient", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

//  CORS istifadə et
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();