using Client.Subscriber;
using Contract.Messages.Orders;
using OrderSystem.DomainModel.Services.TaskWorker;
using OrderSystem.OrderGen.Configs;
using OrderSystem.RabbitMq.Contract.Abstractions;
using OrderSystem.RabbitMq.Contract.Models;
using OrderSystem.TaskMgr.Services;
using OrderSystem.TaskMgr.Services.TaskWorker;

var builder = WebApplication.CreateBuilder(args);

// Configs
builder.Services.AddSingleton(
    builder.Configuration.GetSection("RabbitMq").Get<ConnectionParameters>()!
);
builder.Services.AddSingleton(
    builder.Configuration.GetSection("TaskMgr").Get<TaskMngConfig>()!  // ← TaskMgr, не TaskMng
);

// RabbitMQ
builder.Services.AddSingleton<IMessageSubscriber, RabbitMqMessageSubscriber>();

// TaskWorker Stream
builder.Services.AddSingleton<ITaskWorkerCaller, OrderTaskCaller>();
builder.Services.AddSingleton<TaskWorkerStreamClient>();

// Handlers
builder.Services.AddSingleton<IMessageHandler<ProcessOrderCommand>, OrderCommandHandler>();

// Background Service
builder.Services.AddHostedService<TaskMgrBackgroundService>();

var app = builder.Build();

app.Run();