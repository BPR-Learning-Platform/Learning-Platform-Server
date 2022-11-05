using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Services;

var builder = WebApplication.CreateBuilder(args);
const string allowSpecificOrigins = "_allowRequestsFromBPR-Learning-Platform-Frontend";

// Add services to the container.

var services = builder.Services;

services.AddControllers(options =>
{
    options.Filters.Add<UnhandledExceptionFilterAttribute>();
});
services.AddMemoryCache();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();


// configure DI for application services

services.AddScoped<IUserDAO, UserDAO>();
services.AddScoped<ITaskDAO, TaskDAO>();
services.AddScoped<IGradeDAO, GradeDAO>();
services.AddScoped<IStatisticDAO, StatisticDAO>();

services.AddScoped<UserService>();
services.AddScoped<IUserService, UserServiceWithCache>();
services.AddScoped<ITaskService, TaskService>();
services.AddScoped<GradeService>();
services.AddScoped<IGradeService, GradeServiceWithCache>();
services.AddScoped<IStatisticService, StatisticService>();


services.AddCors(options =>
{
    options.AddPolicy(name: allowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://bpr-learning-platform.github.io",
                                "http://localhost:4200")
                    .AllowAnyHeader();
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(allowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }