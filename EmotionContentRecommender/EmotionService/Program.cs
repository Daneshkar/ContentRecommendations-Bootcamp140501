using EmotionService.Application.Features.MediaItems.Create;
using EmotionService.Infrastructure.Extensions;
using EmotionService.Infrastructure.Persistence.Extensions;
using EmotionService.Infrastructure.Pipeline;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(CreateMediaItemCommand).Assembly);
});

builder.Services.AddValidatorsFromAssembly(
    typeof(CreateMediaItemCommand).Assembly);

builder.Services.AddScoped(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationPipelineBehavior<,>));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

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
