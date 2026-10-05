using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TeamFlow.API.Data;
using TeamFlow.API.Interfaces;
using TeamFlow.API.Middleware;
using TeamFlow.API.Models.Enums;
using TeamFlow.API.Services;
using TeamFlow.API.Services.Background;
using TeamFlow.API.Services.Email;
using TeamFlow.API.Services.Interfaces;
using TeamFlow.API.Services.Payments;

namespace TeamFlow___Project___Team_Management_SaaS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT key is not configured.");

            // The committed development key must never be used outside Development.
            if (!builder.Environment.IsDevelopment() &&
                (jwtKey.Length < 32 ||
                 jwtKey.Contains("DEVELOPMENT_SECRET", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "Jwt:Key must be a strong secret (32+ characters) outside Development. " +
                    "Set it through an environment variable (Jwt__Key) or a secret store.");
            }

            // Rate limiting: limits are applied per client IP, not globally,
            // so one client cannot lock every other user out of logging in.
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy("Auth", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";

                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new
                        {
                            statusCode = StatusCodes.Status429TooManyRequests,
                            message = "Too many attempts. Please wait a minute and try again."
                        },
                        cancellationToken);
                };
            });

            var allowedOrigins =
                builder.Configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>()
                ?? new[] { "http://localhost:5173", "http://localhost:5175" };

            builder.Services.AddCors(setupOption =>
            {
                setupOption.AddPolicy("Client", config =>
                {
                    config.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // Application services
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IOrganizationService, OrganizationService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IPlanLimitService, PlanLimitService>();
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IInvitationService, InvitationService>();
            builder.Services.AddScoped<IEmailService, FakeEmailService>();
            builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();

            builder.Services.AddScoped<IBillingService, BillingService>();
            // Simulated gateway. Swap for a real provider by implementing IPaymentGateway.
            builder.Services.AddScoped<IPaymentGateway, MockPaymentGateway>();

            builder.Services.AddHostedService<DeadlineReminderService>();
            builder.Services.AddHostedService<SubscriptionExpiryService>();

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        // The role and organization are embedded in the token, so a removed
                        // member (or one whose role changed) would otherwise keep their old
                        // access until the token expires. Re-check against the database.
                        OnTokenValidated = async context =>
                        {
                            var principal = context.Principal;

                            var userIdValue =
                                principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                            var organizationIdValue =
                                principal?.FindFirst("OrganizationId")?.Value;
                            var roleValue =
                                principal?.FindFirst(ClaimTypes.Role)?.Value;

                            if (!int.TryParse(userIdValue, out var userId) ||
                                !int.TryParse(organizationIdValue, out var organizationId) ||
                                string.IsNullOrEmpty(roleValue))
                            {
                                context.Fail("Invalid token.");
                                return;
                            }

                            var db = context.HttpContext.RequestServices
                                .GetRequiredService<AppDbContext>();

                            var currentRole = await db.OrganizationMembers
                                .AsNoTracking()
                                .Where(m =>
                                    m.UserId == userId &&
                                    m.OrganizationId == organizationId)
                                .Select(m => (OrganizationRole?)m.Role)
                                .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

                            if (currentRole is null ||
                                currentRole.Value.ToString() != roleValue)
                            {
                                context.Fail("Membership is no longer valid.");
                            }
                        }
                    };
                });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("OwnerOnly", policy =>
                    policy.RequireRole("Owner"));

                options.AddPolicy("AdminOrOwner", policy =>
                    policy.RequireRole("Owner", "Admin"));
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(setupAction =>
            {
                setupAction.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Paste the JWT token here (the Bearer prefix is added automatically)."
                });

                setupAction.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                await next();
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                // Keep the development database in sync with the migrations.
                using var scope = app.Services.CreateScope();
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>()
                    .Database
                    .Migrate();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("Client");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
