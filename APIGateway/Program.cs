var builder = WebApplication.CreateBuilder(args);

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseRouting();

// Optional: Add authentication if you want the gateway to validate tokens
// app.UseAuthentication();
// app.UseAuthorization();

app.MapReverseProxy();

app.Run();