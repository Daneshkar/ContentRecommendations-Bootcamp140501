using AuthService.Infrastructure.Extensions;
using AuthService.Infrastructure.Persistence.Extensions;
using AuthService.Infrastructure.Pipeline;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(AuthService.Application.Features.Auth.Login.LoginCommandHandler).Assembly));

builder.Services.AddValidatorsFromAssembly(
    typeof(AuthService.Application.Features.Auth.Login.LoginCommandValidator).Assembly);

builder.Services.AddScoped(
    typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandling();
await app.ApplyMigrationsAsync();

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