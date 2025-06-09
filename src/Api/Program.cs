using Order_Management.Api;
using Order_Management.Api.Handler;
using Order_Management.Application;
using Order_Management.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApiDocument(settings =>
{
    settings.Title = "Order Management Api";
    settings.DocumentName = "api";
});
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseExceptionHandler(options => { });

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
    settings.DocumentTitle = "Order Management API";
});


app.Map("/", () => Results.Redirect("/api"));

app.MapControllers();

app.Run();

public partial class Program { }
