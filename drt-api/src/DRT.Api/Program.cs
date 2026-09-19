using DRT.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Entra:Authority"];
        options.Audience = builder.Configuration["Entra:Audience"];
    });

builder.Services.AddAuthorization(options =>
{
    // TODO: US-ASK-015 - Confirm the exact group claim name and group object ID for DPP Operations Editor
    // in the DRT RBAC matrix. The policy below is a placeholder until the approved group claim is confirmed.
    options.AddPolicy("DppOperationsEditor", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("groups") // TODO: replace with confirmed group claim and value
    );
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application use cases
builder.Services.AddScoped<DRT.Application.UseCases.CoreAsks.ICreateCoreAskUseCase,
    DRT.Application.UseCases.CoreAsks.CreateCoreAskUseCase>();

builder.Services.AddScoped<DRT.Application.UseCases.CoreAsks.ICancelCoreAskUseCase,
    DRT.Application.UseCases.CoreAsks.CancelCoreAskUseCase>();

// Register repositories
builder.Services.AddScoped<DRT.Application.Abstractions.ICoreAskRepository,
    DRT.Infrastructure.Persistence.Repositories.CoreAskRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DRT API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
