var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CatalogService>("catalogservice");
builder.AddProject<Projects.IdentityService>("identityservice");
builder.AddProject<Projects.InventoryService>("inventoryservice");
builder.AddProject<Projects.TradingService>("tradingservice");

builder.Build().Run();
