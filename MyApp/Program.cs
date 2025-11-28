using Amazon.DynamoDBv2;
using MyApp.Extensions;
using MyApp.Services;

var builder = WebApplication.CreateBuilder(args);

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


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // todo -- check this
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var client = app.Services.GetRequiredService<IAmazonDynamoDB>();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

