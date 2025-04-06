using BuildingBlocks.Common.MassTransit;
using BuildingBlocks.Common.Settings;
using GreenPipes;
using IdentityService.Entities;
using IdentityService.Exceptions;
using IdentityService.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
builder.Services.Configure<IdentitySettings>(builder.Configuration.GetSection(nameof(IdentitySettings)))
               .AddDefaultIdentity<ApplicationUser>()
               .AddRoles<ApplicationRole>()
               .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
               (
                   mongoDbSettings?.ConnectionString,
                   serviceSettings?.ServiceName
               );

builder.Services.AddMassTransitWithRabbitMq(builder.Configuration, retryConfigurator =>
{
    retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
    retryConfigurator.Ignore(typeof(UnknownUserException));
    retryConfigurator.Ignore(typeof(InsufficientFundsException));
});

// Add IdentityServer
IdentityServerSettings identityServerSettings = builder.Configuration.GetSection(nameof(IdentityServerSettings)).Get<IdentityServerSettings>() ?? throw new ArgumentNullException();

var identityServerBuilder = builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseSuccessEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseErrorEvents = true;
})
.AddAspNetIdentity<ApplicationUser>()
.AddInMemoryApiScopes(identityServerSettings.ApiScopes)
.AddInMemoryApiResources(identityServerSettings.ApiResources)
.AddInMemoryClients(identityServerSettings.Clients)
.AddInMemoryIdentityResources(identityServerSettings.IdentityResources);

if (!builder.Environment.IsDevelopment())
{
    var identitySettings = builder.Configuration.GetSection(nameof(IdentitySettings))
                                        .Get<IdentitySettings>() ?? throw new ArgumentNullException();
    var cert = X509Certificate2.CreateFromPemFile(
        identitySettings.CertificateCerFilePath,
        identitySettings.CertificateKeyFilePath
    );

    identityServerBuilder.AddSigningCredential(cert);
}
// end Add IdentityServer

// for local api authentication between services, not for external clients
builder.Services.AddLocalApiAuthentication();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(options =>
    {
        options.WithOrigins(builder.Configuration["AllowedOrigin"]!)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
