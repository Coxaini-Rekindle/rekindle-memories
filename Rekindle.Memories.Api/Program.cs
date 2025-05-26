using System.Text.Json;
using System.Text.Json.Serialization;
using Rekindle.Exceptions.Api;
using Rekindle.Exceptions.Api.Extensions;
using Rekindle.Memories.Application;
using Rekindle.Memories.Infrastructure;
using Rekindle.Memories.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

// Configure JSON serialization options for automatic enum to string conversion
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

if (allowedOrigins == null || allowedOrigins.Length == 0)
    throw new ArgumentNullException(nameof(allowedOrigins), "Allowed origins must be configured.");

builder.Services.AddCors(options =>
{
  options.AddPolicy("Default",
        corsPolicyBuilder =>
        {
            corsPolicyBuilder
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition", "*");
        });
});

builder.Services.AddAuthorization();
builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Start the Rebus message bus
app.Services.StartRebus();

app.UseSwaggerUI(c =>
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => { o.SwaggerEndpoint("/openapi/v1.json", "Rekindle Memories API"); });
});

app.UseCors("Default");

app.UseExceptionHandlingMiddleware();
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.Run();