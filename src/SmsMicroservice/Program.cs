using SmsMicroservice.AsyncDataService;
using SmsMicroservice.AsyncDataService.MessageQueueClient;
using SmsMicroservice.EventBus;
using SmsMicroservice.EventProcessor;
using SmsMicroservice.Logger;
using SmsMicroservice.Logger.LoggerClient;
using SmsMicroservice.Messenger;
using SmsMicroservice.SyncDataService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<IHttpSmsDataClient, HttpSmsDataClient>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddSingleton<IEventProcessor, EventProcessor>();
builder.Services.AddSingleton<IMessageQueue, RabbitMessageQueue>(); 
builder.Services.AddScoped<IEventBus, EventBus>();
builder.Services.AddScoped<ILoggerLibrary, CustomLogger>();
builder.Services.AddHostedService<MessageQueueListener>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
