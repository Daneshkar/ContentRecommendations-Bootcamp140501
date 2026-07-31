using EmotionService.Infrastructure.Extensions;
using EmotionService.Infrastructure.Persistence.Extensions;
using EmotionService.Infrastructure.Pipeline;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped(
    typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandling();               // ① اول از همه
await app.ApplyMigrationsAsync();         // ② Migration خودکار در Startup

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
