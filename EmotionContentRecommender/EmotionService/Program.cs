using EmotionService.Application.Features.MediaItems.Create;
using EmotionService.Infrastructure.Extensions;
using EmotionService.Infrastructure.Persistence.Extensions;
using EmotionService.Infrastructure.Pipeline;
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

builder.Services.AddScoped(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationPipelineBehavior<,>));

builder.Services.AddInfrastructure(builder.Configuration);

// TODO: JWT authentication is temporarily disabled due to a configuration issue.
// !The integration flow is implemented, but the JWT settings/validation must be reviewed
// !with the AuthService implementation before enabling it again.
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