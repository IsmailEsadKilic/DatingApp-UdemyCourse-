
using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task ClearConnections(DataContext context)
        {
            context.Connections.RemoveRange(context.Connections);
            await context.SaveChangesAsync();
        }

        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var Users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

            var roles = new List<AppRole>
            {
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"}
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            foreach (var user in Users)
            {
                try {
                    Console.WriteLine("NOW TRYING TO CREATE USER: " + user.UserName);
                    user.UserName = user.UserName.ToLower();

                    user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
                    user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);

                    await userManager.CreateAsync(user, "Pa$$w0rd");

                    await userManager.AddToRoleAsync(user, "Member");           
                }
                catch(Exception ex){
                    Console.WriteLine(ex.Message);
                }
            }

            var admin = new AppUser
            {
                UserName = "admin"
            };
            try{
                await userManager.CreateAsync(admin, "Pa$$w0rd");
                await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
            }
            catch(Exception ex){
                Console.WriteLine(ex.Message);
                Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAA/nAAAAAAAAAAAAAAAAAAAAAAAAAAAA/nAAAAAAAAAAAAAAAAAAAAAAAAAAAA/nAAAAAAAAAAAAAAAAAAAAAAAAAAAA/nAAAAAAAAAAAAAAAAAAAAAAAAAAAA/nAAAAAAAAAAAAAAAAAAAAAAAAAAAA/nAAAAAAAAAAAAAAAAAAAAAAAAAAAA/nAAAAAAAAAAAAAAAAAAAAAAAAAAAA/n");
            }
        }
    }
}