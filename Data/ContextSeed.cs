using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulletinBoard.Models;

namespace BulletinBoard.Data
{
    public static class ContextSeed
    {
        public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.SuperAdmin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.Moderator.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.Basic.ToString()));
        }



        public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager)
        {
            // 1) SuperAdmin
            var superAdminEmail = "SuperAdmin@gmail.com";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    UserName = "SuperAdmin",
                    Email = superAdminEmail,
                    FirstName = "JINSOO",
                    LastName = "LEE",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    UsernameChangeLimit = 10  // 명시적으로 채워주기
                };

                var result = await userManager.CreateAsync(superAdminUser, "a1324");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, Enums.Roles.Basic.ToString());
                    await userManager.AddToRoleAsync(superAdminUser, Enums.Roles.Moderator.ToString());
                    await userManager.AddToRoleAsync(superAdminUser, Enums.Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(superAdminUser, Enums.Roles.SuperAdmin.ToString());
                }
                else
                {
                    // 디버깅용 로그(선택)
                    // throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }

            // 2) Admin
            var adminEmail = "Admin@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "Admin",
                    Email = adminEmail,
                    FirstName = "JINSOO",
                    LastName = "LEE",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    UsernameChangeLimit = 10
                };

                var result = await userManager.CreateAsync(adminUser, "a1324");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Enums.Roles.Basic.ToString());
                    await userManager.AddToRoleAsync(adminUser, Enums.Roles.Moderator.ToString());
                    await userManager.AddToRoleAsync(adminUser, Enums.Roles.Admin.ToString());
                }
            }

            // 3) Moderator
            var moderatorEmail = "Moderator@gmail.com";
            var moderatorUser = await userManager.FindByEmailAsync(moderatorEmail);

            if (moderatorUser == null)
            {
                moderatorUser = new ApplicationUser
                {
                    UserName = "Moderator",
                    Email = moderatorEmail,
                    FirstName = "JINSOO",
                    LastName = "LEE",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    UsernameChangeLimit = 10
                };

                var result = await userManager.CreateAsync(moderatorUser, "a1324");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(moderatorUser, Enums.Roles.Basic.ToString());
                    await userManager.AddToRoleAsync(moderatorUser, Enums.Roles.Moderator.ToString());
                }
            }

            // 4) Basic
            var basicEmail = "Basic@gmail.com";
            var basicUser = await userManager.FindByEmailAsync(basicEmail);

            if (basicUser == null)
            {
                basicUser = new ApplicationUser
                {
                    UserName = "Basic",
                    Email = basicEmail,
                    FirstName = "JINSOO",
                    LastName = "LEE",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    UsernameChangeLimit = 10
                };

                var result = await userManager.CreateAsync(basicUser, "a1324");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(basicUser, Enums.Roles.Basic.ToString());
                }
            }
        }

    }
}
