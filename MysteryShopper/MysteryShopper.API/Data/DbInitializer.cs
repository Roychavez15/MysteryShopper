using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MysteryShopper.API.Controllers;
using MysteryShopper.API.Domain;
using MysteryShopper.API.Domain.Identity;

namespace MysteryShopper.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();
            var provider = scope.ServiceProvider;

            var context = provider.GetRequiredService<AppDbContext>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1️⃣ Aplicar migraciones
            await context.Database.MigrateAsync();

            // 2️⃣ Crear roles
            string[] roles = { Roles.Admin, Roles.Client, Roles.Evaluator };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation($"Rol '{role}' creado.");
                }
            }

            // 3️⃣ Crear usuario admin
            var adminEmail = "admin@mysteryshopper.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin123$");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, Roles.Admin);
            }

            // 4️⃣ Crear compañía
            var company = await context.Companies.FirstOrDefaultAsync(c => c.Name == "KFC");
            if (company == null)
            {
                company = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "KFC",
                    Notes = "Compañía de ejemplo para Mystery Shopper"
                };
                context.Companies.Add(company);
                await context.SaveChangesAsync();
                logger.LogInformation("Compañía 'KFC' creada.");
            }

            // 5️⃣ Crear cliente asociado a la compañía
            var clientEmail = "client@kfc.local";
            var client = await userManager.FindByEmailAsync(clientEmail);
            if (client == null)
            {
                client = new ApplicationUser
                {
                    UserName = clientEmail,
                    Email = clientEmail,
                    EmailConfirmed = true,
                    CompanyId = company.Id
                };
                var result = await userManager.CreateAsync(client, "Client123$");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(client, Roles.Client);
            }

            // 6️⃣ Crear agencia
            var agency = await context.Agencies.FirstOrDefaultAsync(a => a.Name == "KFC Amazonas");
            if (agency == null)
            {
                agency = new Agency
                {
                    Id = Guid.NewGuid(),
                    Name = "KFC Amazonas",
                    CompanyId = company.Id,
                    Address = "Av. Amazonas y Colón"
                };
                context.Agencies.Add(agency);
                await context.SaveChangesAsync();
                logger.LogInformation("Agencia creada.");
            }

            // 7️⃣ Crear evaluador
            var evaluatorEmail = "evaluator@demo.local";
            var evaluator = await userManager.FindByEmailAsync(evaluatorEmail);
            if (evaluator == null)
            {
                evaluator = new ApplicationUser
                {
                    UserName = evaluatorEmail,
                    Email = evaluatorEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(evaluator, "Evaluator123$");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(evaluator, Roles.Evaluator);
            }

            // 8️⃣ Crear plantilla de encuesta con CompanyId asignado
            var survey = await context.SurveyTemplates.FirstOrDefaultAsync(s => s.Title == "Atención al Cliente");
            if (survey == null)
            {
                survey = new SurveyTemplate
                {
                    Id = Guid.NewGuid(),
                    Title = "Atención al Cliente",
                    Description = "Evaluación de atención en mostrador",
                    CompanyId = company.Id
                };

                // primero se guarda la plantilla sin preguntas
                context.SurveyTemplates.Add(survey);
                await context.SaveChangesAsync();

                // ahora se agregan las preguntas asociadas
                var questions = new List<Question>
                {
                    new Question
                    {
                        Id = Guid.NewGuid(),
                        Text = "¿El personal saludó al ingresar?",
                        Type = QuestionType.YesNo,
                        Weight = 10,
                        SurveyTemplateId = survey.Id
                    },
                    new Question
                    {
                        Id = Guid.NewGuid(),
                        Text = "¿Qué tan limpia estaba la zona de atención?",
                        Type = QuestionType.Rating1to5,
                        Weight = 20,
                        SurveyTemplateId = survey.Id
                    },
                    new Question
                    {
                        Id = Guid.NewGuid(),
                        Text = "Comentarios adicionales",
                        Type = QuestionType.Text,
                        Weight = 0,
                        SurveyTemplateId = survey.Id
                    }
                };

                context.Questions.AddRange(questions);
                await context.SaveChangesAsync();

                logger.LogInformation("Plantilla de encuesta y preguntas creadas.");
            }

            // 9️⃣ Asignar encuesta al evaluador
            if (!await context.SurveyAssignments.AnyAsync())
            {
                var assignment = new SurveyAssignment
                {
                    Id = Guid.NewGuid(),
                    SurveyTemplateId = survey.Id,
                    AgencyId = agency.Id,
                    EvaluatorUserId = evaluator.Id,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    Completed = false
                };
                context.SurveyAssignments.Add(assignment);
                await context.SaveChangesAsync();
                logger.LogInformation("Encuesta asignada al evaluador.");
            }

            logger.LogInformation("✅ SEED COMPLETO sin errores de FK.");
        }
    }
}
