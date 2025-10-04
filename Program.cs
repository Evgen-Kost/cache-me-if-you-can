var builder = Host.CreateApplicationBuilder(args);
builder.AddMaintainServices();
builder.AddProcessServices();

var host = builder.Build();

host.Run();