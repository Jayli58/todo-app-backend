using Amazon.DynamoDBv2;
using MyApp.Exceptions;
using MyApp.Extensions;
using MyApp.Services;
//using Amazon.Lambda.AspNetCoreServer.Hosting;
//using Amazon.Lambda.AspNetCoreServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// Needed for CurrentUser to access HttpContext
builder.Services.AddHttpContextAccessor();
// Make CurrentUser request-scoped; just like request-scoped bean in java spring
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// import DynamoDB settings and register services
builder.Services.AddDynamoDb(builder.Configuration);

// Enable Swagger
builder.Services.AddMySwagger();

// AddControllers -- api only without views
// Configure JSON options to serialize enums as strings
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add Cognito authentication
builder.Services.AddCognitoAuth(builder.Configuration);

// Configure CORS to allow requests from frontend
builder.Services.AddMyCors(builder.Configuration);

// Register global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// Register ProblemDetails middleware
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use custom exception handler middleware
app.UseExceptionHandler();

var client = app.Services.GetRequiredService<IAmazonDynamoDB>();
//var tables = await client.ListTablesAsync();
//Console.WriteLine("Tables visible to .NET: " + string.Join(", ", tables.TableNames));

//app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(CorsExtensions.GetPolicyName());
app.UseAuthentication();
app.UseAuthorization();

//app.UseStaticFiles();

// Map controller routes
app.MapControllers();

app.Run();

