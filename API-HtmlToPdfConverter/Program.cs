using API_HtmlToPdfConverter.Interfaces;
using API_HtmlToPdfConverter.Services;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
//builder.Services.AddScoped<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());

builder.Services.AddScoped(x => new BlobServiceClient(builder.Configuration.GetConnectionString("AzureStorageAccount")));
builder.Services.AddScoped<IAzureStorageAccountService, AzureStorageAccountService>();
builder.Services.AddScoped<IConvertService, ConvertService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy("AllowReactApp",
    p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors("AllowReactApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
