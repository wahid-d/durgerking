using System.Text.Json.Serialization;
using DurgerKing.Dtos;
using DurgerKing.Data;
using Durgerking.Filters;
using DurgerKing.Services;
using durgerking.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using DurgerKing;
using Durgerking.Services;
using DurgerKing.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddMvcOptions(options => 
        options.Filters.Add<AsyncFluentAutoValidation>(AsyncFluentAutoValidation.OrderLowerThanModelStateInvalidFilter))
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

builder.Services.AddLocalization();
builder.Services.AddTransient<IValidator<CreateProductDto>, CreateProductValidator>();
builder.Services.AddTransient<IValidator<UpdateProductDto>, UpdateProductValidator>();
builder.Services.AddTransient<IBotResponseService, BotResponseService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ILocalizationHandler, LocalizationHandler>();
builder.Services.AddTransient<IUpdateHandler, UpdateHandler>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddHostedService<BotStartingBackgroundService>();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ITelegramBotClient, TelegramBotClient>(
    p => new TelegramBotClient(builder.Configuration.GetValue("BotApiKey", string.Empty)));

builder.Services.AddDbContext<IAppDbContext, AppDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddDbContext<UserIdentityContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;

}).AddEntityFrameworkStores<UserIdentityContext>();

builder.Services.AddHttpClient<IAddressService, AddressService>(
    configureClient: c => c.BaseAddress = new Uri(builder.Configuration.GetValue("Geocode:BaseUrl", string.Empty)));

var app = builder.Build();

await SeedUserAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealth();
app.MapControllers();

app.Run();

async Task SeedUserAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManeger = scope.ServiceProvider
        .GetRequiredService<UserManager<AppUser>>();
    
    var config = scope.ServiceProvider
        .GetRequiredService<IConfiguration>();

    var roleManger = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    
    var rolesJson= config.GetSection("Seed:Roles").Get<List<string>>();

    foreach(var role in rolesJson)
    {
        await roleManger.CreateAsync(new IdentityRole<Guid>(role));
    }

    var userJson = config.GetSection("Seed:Users").Get<List<AppUserDto>>();
    Console.WriteLine("userJson: " + userJson.Count);
    foreach(var user in userJson)    
    {
        if(user is null)
            Console.WriteLine("User is null");

        var userResult = await userManeger.CreateAsync(
            user : new AppUser()
            {
                UserName = user.Email,
                Fullname = user.Fullname,
                Email = user.Email
            },
            password: user.Password
        );
        var createdUser = await userManeger.FindByNameAsync(user.Email);
        
        if(createdUser != null)
            await userManeger.AddToRoleAsync(createdUser, user.Role);
    }
}

public class AppUserDto
{
    public string UserName { get; set;}
    public string Fullname { get; set;}
    public string Email { get; set;}
    public string Password { get; set;}
    public string Role { get; set;}
    public DateTime Birthdate { get; set;} = DateTime.UtcNow;
}