using Microsoft.EntityFrameworkCore;
using Ork.Helpers;
using Ork.Services.Payment;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Same registrations as Ork/Ork/Program.cs (payer mode). No Database.Migrate() -
// this points at a copy of an existing payer database that already has the schema.
builder.Services.AddDbContext<PayerDataContext>(options =>
	options.UseNpgsql(builder.Configuration.GetValue<string>("Payer_DB_Connection_String")));
builder.Services.AddScoped<IPayerService, PayerService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICustomerAdminService, CustomerAdminService>();


var app = builder.Build();

app.MapControllers();

app.Run();
