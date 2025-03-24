using IdentityService.Entities;
using IdentityService.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
builder.Services.Configure<IdentitySettings>(builder.Configuration.GetSection(nameof(IdentitySettings)))
               .AddDefaultIdentity<ApplicationUser>()
               .AddRoles<ApplicationRole>()
               .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
               (
                   mongoDbSettings.ConnectionString,
                   serviceSettings.ServiceName
               );


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
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
