using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinimalApi_Test.Context;
using MinimalApi_Test.DTOs.User;
using MinimalApi_Test.Endpoints;
using MinimalApi_Test.Entities.User;
using MinimalApi_Test.Handlers.User;
using MinimalApi_Test.Mapping;
using MinimalApi_Test.Repositories.Interfaces;
using MinimalApi_Test.Repositories.Services;
using MinimalApi_Test.Result;
using MinimalApi_Test.Services;
using MinimalApi_Test.Services.Interfaces;
using MinimalApi_Test.Validators.User;
using System.Net;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using StackExchange.Profiling;

#region Containers

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomMiniProfiler();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // محدودیت سایز 10 مگابایت
});

builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
AddSwagger(builder.Services);
builder.Services.AddAntiforgery();

builder.Services.AddCors(o =>
{
    o.AddPolicy("CorsPolicy", b =>
        b.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

#region Logging 

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddFile("Logs/app-{Date}.txt");

#endregion

#region IoC

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserDtoValidator>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddSingleton<ItemService>();
builder.Services.AddSingleton<ProductService>();

#endregion

#region Authentication

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();

#endregion

#region DataBase Context

// builder.Services.AddDbContext<AppDbContext>(
//     options => options.UseSqlServer(builder.Configuration["ConnectionStrings:SqlConnectionString"]));

// builder.Services.AddDbContext<AppDbContext>(options =>
// {
//     options.UseSqlServer(builder.Configuration["ConnectionStrings:SqlConnectionString"])
//         .EnableSensitiveDataLogging(); // برای مشاهده مقادیر پارامترها
// });

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:SqlConnectionString"]);
    options.AddMiniProfilerToDbContext();
});
#endregion

#endregion

#region PipeLines

var app = builder.Build();

app.UseCustomMiniProfiler();

// Configure the HTTP request pipeline.
app.Use(async (context, next) =>
{
    Console.WriteLine($"The request was received: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Reply sent: {context.Response.StatusCode}");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
LoggerService.ConfigureLogger(loggerFactory.CreateLogger("App"));

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapProductEndpoints();

#endregion

#region Endpoints

#region Authorize

app.MapGet("api/secure", [Authorize]() => "This is a secure endpoint");

app.MapGet("api/admin", [Authorize(Roles = "Admin")]()
    => "Welcome Admin!");

app.MapGet("api/user", [Authorize(Roles = "User")]()
    => "Welcome User!");

#endregion

#region Files

// تنظیم مسیر ذخیره فایل‌ها در wwwroot
const string uploadDirectory = "uploads";
var webRootPath = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var uploadsPath = Path.Combine(webRootPath, uploadDirectory);

if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.MapGet("api/download/{fileName}", async (string fileName) =>
{
    var filePath = Path.Combine("wwwroot/download", fileName);

    if (!File.Exists(filePath))
        return Results.NotFound();

    var fileBytes = await File.ReadAllBytesAsync(filePath);

    return Results.File(fileBytes, "application/octet-stream", fileName, enableRangeProcessing: true);
});

app.MapPost("/api/upload", async Task<IResult> (IFormFile file) =>
{
    try
    {
        if (file.Length == 0)
        {
            return Results.BadRequest(new
            {
                Success = false,
                Message = "فایل خالی است"
            });
        }

        // بررسی پسوند فایل
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            return Results.BadRequest(ResultCustom<FileUploadResult>.Failure(
                $"پسوند فایل{fileExtension} مجاز نیست. پسوندهای مجاز: {string.Join(", ", allowedExtensions)}"));

        // ایجاد نام یکتا برای فایل
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsPath, uniqueFileName);

        // ذخیره فایل
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUploadResult = new FileUploadResult
        {
            FileName = uniqueFileName,
            FilePath = filePath,
            Message = "فایل با موفقیت آپلود شد"
        };

        return Results.Ok(ResultCustom<FileUploadResult>.Success(fileUploadResult));
    }
    catch (Exception)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
}).DisableAntiforgery();

app.MapPost("/api/upload-multiple", async Task<IResult> (IFormFileCollection files) =>
{
    try
    {
        if (files.Count == 0)
            return Results.BadRequest(ResultCustom<FileUploadResult>.Failure("فایلی انتخاب نشده است."));

        var uploadedFiles = new List<FileUploadResult>();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };

        foreach (var file in files)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                continue;

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            uploadedFiles.Add(new FileUploadResult
            {
                FileName = uniqueFileName,
                FilePath = filePath,
                Message = "فایل با موفقیت آپلود شد"
            });
        }

        return Results.Ok(ResultCustom<List<FileUploadResult>>.Success(uploadedFiles));
    }
    catch (Exception)
    {
        return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    }
}).DisableAntiforgery();

#endregion

#region Items

#region Tests

//// تعریف یک لیست در حافظه برای ذخیره داده ها
//var items = new List<string>();

//app.MapGet("/", () => "سلام، به Minimal API خوش آمدید!");

//// مسیر GET: دریافت تمام آیتم‌ها    
//app.MapGet("/items", () => items);

//// مسیر POST: افزودن آیتم جدید
//app.MapPost("/items", (string item) =>
//{
//    items.Add(item);
//    return Results.Created($"/items/{items.Count - 1}", item);
//});

//// مسیر PUT: ویرایش یک آیتم
//app.MapPut("/items/{index}", (int index, string updatedItem) =>
//{
//    if (index < 0 || index >= items.Count)
//    {
//        return Results.NotFound("آیتم یافت نشد");
//    }
//    items[index] = updatedItem;
//    return Results.Ok(updatedItem);
//});

//// مسیر DELETE: حذف یک آیتم
//app.MapDelete("/items/{index}", (int index) =>
//{
//    if (index < 0 || index >= items.Count)
//    {
//        return Results.NotFound("آیتم پیدا نشد");
//    }
//    items.RemoveAt(index);
//    return Results.Ok("آیتم حذف شد");
//});

#endregion

#region Get

app.MapGet("/api/items", (ItemService service) => service.GetAll());

app.MapGet("/api/items/{index}", (ItemService service, int index) =>
{
    var item = service.Get(index);
    return item is null ? Results.NotFound("آیتم پیدا نشد") : Results.Ok(item);
});

app.MapGet("/api/posts/{*rest}", (string rest) => $"Routing to {rest}");

#endregion

#region Post

app.MapPost("/api/items", (ItemService service, string item) =>
{
    service.Add(item);
    return Results.Created($"/items/{item}", item);
});

#endregion

#region Put

app.MapPut("/api/items/{index}", (ItemService service, int index, string updatedItem)
    => service.Update(index, updatedItem) ? Results.Ok((object?)updatedItem) : Results.NotFound("آیتم پیدا نشد"));

#endregion

#region Delete

app.MapDelete("/api/items/{index}", (ItemService service, int index)
    => service.Delete(index) ? Results.Ok("آیتم حذف شد") : Results.NotFound("آیتم پیدا نشد"));

#endregion

#endregion

#region User

var users = app.MapGroup("/api/users")
    .WithTags("Users")
    .WithOpenApi();

// GET /api/users
users.MapGet("/", UserHandlers.GetAllUsers)
    .WithName("GetAllUsers")
    .Produces<ResultCustom<List<UserDto>>>(200)
    .ProducesProblem(500);

// GET /api/users/{id}
users.MapGet("/{id:int}", UserHandlers.GetUserById)
    .WithName("GetUserById")
    .Produces<ResultCustom<UserDto>>(200)
    .ProducesProblem(404)
    .ProducesProblem(500);

// POST /api/users
users.MapPost("/", UserHandlers.CreateUser)
    .WithName("CreateUser")
    .Produces<ResultCustom<UserDto>>(201)
    .ProducesProblem(400)
    .ProducesProblem(500);

// PUT /api/users/{id}
users.MapPut("/{id:int}", UserHandlers.UpdateUser)
    .WithName("UpdateUser")
    .Produces<ResultCustom<UserDto>>(200)
    .ProducesProblem(400)
    .ProducesProblem(404)
    .ProducesProblem(500);

// DELETE /api/users/{id}
users.MapDelete("/{id:int}", UserHandlers.DeleteUser)
    .WithName("DeleteUser")
     .Produces<ResultCustom<bool>>(200)
    .ProducesProblem(404)
    .ProducesProblem(500);

// POST /api/users/login
users.MapPost("/login", UserHandlers.Login)
    .WithName("LoginUser")
    .Produces<LoginResponse>(200)
    .ProducesProblem(400)
    .ProducesProblem(500);

//POST /api/users/search
users.MapPost("/search", UserHandlers.SearchUsers)
    .WithName("SearchUsers")
    .Produces<ResultCustom<SearchUsersResult>>(200)
    .ProducesProblem(400)
    .ProducesProblem(500);

// PATCH /api/users/{id}
// users.MapPatch("/{id:int}", UserHandlers.PatchUser)
//     .WithName("PatchUser")
//     // .Produces<ResultCustom<UserDto>>(200)
//     .ProducesProblem(400)
//     .ProducesProblem(404)
//     .ProducesProblem(500);

#endregion

#region MiniProfilerTests

app.MapGet("/miniprofiler/", () =>
{
    using (MiniProfiler.Current.Step("Processing Root Request"))
    {
        return "Hello, MiniProfiler with Minimal API!";
    }
});

// تست Endpoint دیگر
app.MapGet("/miniprofiler/slow", async () =>
{
    using (MiniProfiler.Current.Step("Simulating Slow Endpoint"))
    {
        await Task.Delay(2000); // شبیه‌سازی عملیات زمان‌بر
        return "This was a slow request!";
    }
});

#endregion

#endregion

app.Run();

#region Utilities

void AddSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(o =>
    {
        o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 1234sddsw'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        o.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });

        o.SwaggerDoc("v1", new OpenApiInfo()
        {
            Version = "v1",
            Title = "Rayvarz Api"
        });

        //o.UseInlineDefinitionsForEnums();
    });
}

#endregion