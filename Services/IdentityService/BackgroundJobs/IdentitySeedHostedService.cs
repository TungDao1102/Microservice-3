
using IdentityService.Constants;
using IdentityService.Entities;
using IdentityService.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IdentityService.BackgroundJobs
{
    public class IdentitySeedHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IdentitySettings settings;
        public IdentitySeedHostedService(
           IServiceScopeFactory serviceScopeFactory,
           IOptions<IdentitySettings> identityOptions)
        {
            _serviceScopeFactory = serviceScopeFactory;
            settings = identityOptions.Value;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await CreateRoleIfNotExistsAsync(Roles.Admin, roleManager);
            await CreateRoleIfNotExistsAsync(Roles.Player, roleManager);

            var adminUser = await userManager.FindByEmailAsync(settings.AdminUserEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = settings.AdminUserEmail,
                    Email = settings.AdminUserEmail
                };

                await userManager.CreateAsync(adminUser, settings.AdminUserPassword);
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private static async Task CreateRoleIfNotExistsAsync(string role, RoleManager<ApplicationRole> roleManager)
        {
            var roleExists = await roleManager.RoleExistsAsync(role);

            if (!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }
    }
}
