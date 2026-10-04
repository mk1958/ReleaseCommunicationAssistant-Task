using Microsoft.EntityFrameworkCore;
using ReleaseCommunicationAssistant_Task.Models;
using OpenAI.Responses;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
#pragma warning disable OPENAI001

builder.Services.AddSingleton<OpenAI001.Responses.ResponsesClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var apiKey = configuration["OpenAI:ApiKey"];

    if (string.IsNullOrWhiteSpace(apiKey))
    {
        throw new InvalidOperationException(
            "OpenAI API key is not configured.");
    }

    return new OpenAI.Responses.ResponsesClient(apiKey);
});

#pragma warning restore OPENAI001

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();