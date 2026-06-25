using GadgetsOnline.Data;
using GadgetsOnline.Data.Reports;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Shared ADO.NET data layer. The reporting app is read-only: it connects to
// the same database as the store but does NOT bootstrap schema/seed (the store
// owns that), so there is no Database.Initialize call here. The password comes
// from user secrets / environment and is merged in, keeping it out of
// appsettings.json while the database name stays visible there.
var baseConnectionString = builder.Configuration.GetConnectionString("GadgetsOnlineEntities");
var dbPassword = builder.Configuration["DbPassword"];
var connectionString = Database.BuildConnectionString(baseConnectionString, dbPassword);
builder.Services.AddSingleton(new Database(connectionString));
builder.Services.AddScoped<ReportRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reports}/{action=Index}/{id?}");

app.Run();
