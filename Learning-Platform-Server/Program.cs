using Learning_Platform_Server.Services;

var builder = WebApplication.CreateBuilder(args);
var AllowSpecificOrigins = "_allowRequestsFromBPR-Learning-Platform-Frontend";

// Add services to the container.

var services = builder.Services;

services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// configure DI for application services
services.AddScoped<IUserService, UserService>();
services.AddScoped<ITaskService, TaskService>();

services.AddCors(options =>
{
    options.AddPolicy(name: AllowSpecificOrigins,
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

app.UseCors(AllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }