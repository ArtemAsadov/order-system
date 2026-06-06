using Client.Publisher;
using OrderSystem.OrderGen.Configs;
using OrderSystem.OrderGen.Services;
using OrderSystem.RabbitMq.Contract.Abstractions;
using OrderSystem.RabbitMq.Contract.Models;

var builder = WebApplication.CreateBuilder(args);


// Congigs
builder.Services.AddSingleton(
    builder.Configuration.GetSection("RabbitMq").Get<ConnectionParameters>()!
);
builder.Services.AddSingleton(
    builder.Configuration.GetSection("Generator").Get<GeneratorConfig>()!
);

//rbMq
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
builder.Services.AddSingleton<OrderPublisher>();

//srvs
builder.Services.AddSingleton<OrderGeneratorService>();

//api
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // любой источник
              .AllowAnyMethod()    // GET, POST, PUT, DELETE и т.д.
              .AllowAnyHeader();   // любые заголовки
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();

app.Lifetime.ApplicationStopped.Register(() =>
{
    var generator = app.Services.GetService<OrderGeneratorService>();
    generator?.Stop();
    generator?.Dispose();
});

app.Run();
