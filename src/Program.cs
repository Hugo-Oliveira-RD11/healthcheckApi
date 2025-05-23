
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
  .AddRedis(builder.Configuration["connection:redis"]!, name:"cache")
  .AddNpgSql(builder.Configuration["connection:postgres"]!, name:"database")
  .AddMongoDb(sp => new MongoClient(builder.Configuration["connection:mongo"]!), name: "mongodb");

builder.Services.AddHealthChecksUI(op => {
  op.SetEvaluationTimeInSeconds(5);
  op.MaximumHistoryEntriesPerEndpoint(20);
  op.AddHealthCheckEndpoint("dashboard health check","/health");
  })
  .AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecksUI(options =>
    {
        options.UIPath = "/health/dashboard";
    });
});
app.Run();
