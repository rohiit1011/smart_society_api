using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.Helpers;
using SocietyManagementAPI.Interface;
using SocietyManagementAPI.Service;


// PostgreSQL timestamp and JSON settings
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
//NpgsqlConnection.GlobalTypeMapper.UseJsonNet();

var builder = WebApplication.CreateBuilder(args);

/*
 // In Startup / Program.cs (configuration)
services.AddHangfire(cfg => cfg.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));
services.AddHangfireServer();

// schedule recurring job
RecurringJob.AddOrUpdate<IMaintenanceService>("apply-late-penalties", svc => svc.ApplyLatePenaltiesForDueRunsAsync(), Cron.Daily);

 */


builder.Services.AddDbContext<SocietyContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection1")));


builder.Services.AddScoped<CommonService>();
builder.Services.AddScoped<ICommonService, CommonService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CommonService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISocietyDocumentService, SocietyDocumentService>();
builder.Services.AddScoped<IResidentService, ResidentService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Society API", Version = "v1" });
    c.OperationFilter<FileUploadOperationFilter>(); // Only affects IFormFile methods
});




builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB file size limit
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("DevCorsPolicy");



app.UseStaticFiles(); // default wwwroot folder

// or custom folder
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//        Path.Combine(builder.Environment.ContentRootPath, "Uploads")),
//    RequestPath = "/uploads"
//});



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
