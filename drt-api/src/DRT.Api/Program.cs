using DRT.Application.Abstractions;
using DRT.Application.UseCases.CoreAsks;
using DRT.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AzureAd:Authority"];
        options.Audience = builder.Configuration["AzureAd:Audience"];
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCoreAskCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CancelCoreAskCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SearchCoreAsksQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetCoreAskSummaryQueryValidator>();

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application use cases
builder.Services.AddScoped<ICreateCoreAskUseCase, CreateCoreAskUseCase>();
builder.Services.AddScoped<ICancelCoreAskUseCase, CancelCoreAskUseCase>();
builder.Services.AddScoped<ISearchCoreAsksUseCase, SearchCoreAsksUseCase>();
builder.Services.AddScoped<IGetCoreAskSummaryUseCase, GetCoreAskSummaryUseCase>();

// Infrastructure ports
builder.Services.AddScoped<ICoreAskRepository, CoreAskRepository>();
builder.Services.AddScoped<ICoreAskSearchRepository, CoreAskSearchRepository>();
builder.Services.AddScoped<ICoreAskSummaryRepository, CoreAskSummaryRepository>();
builder.Services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IActorResolver, ActorResolver>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
builder.Services.AddHttpContextAccessor();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
