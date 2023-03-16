using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using WebApiAutores.Filters;
using WebApiAutores.Middlewares;
using WebApiAutores.Services;
using WebApiAutores.Utilities;

[assembly:ApiConventionType(typeof(DefaultApiConventions))]
namespace WebApiAutores
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(opt =>
            {
                opt.Filters.Add(typeof(FilterException));
                opt.Conventions.Add(new SwaggerAgrupaVersion());

            }).AddJsonOptions(x => 
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
            ).AddNewtonsoftJson();

            services.AddDbContext<ApplicationDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection"))
            );

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["keyJWT"])),
                    ClockSkew = TimeSpan.Zero
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "WebAPIAutores", 
                    Version = "v1",
                    Description = "Este es un web API para trabajar con autores y libros",
                    Contact = new OpenApiContact
                    {
                        Email = "jhonjameshernandez@gmail.com",
                        Name = "Jhon James",
                        Url = new Uri("https://jhon.blog")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT"
                    }
                });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebAPIAutores", Version = "v2" });

                c.OperationFilter<AgregarParametroHATEOAS>();
                c.OperationFilter<AgregarParametroVersion>();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                //Documentacion
                var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var rutaXML = Path.Combine(AppContext.BaseDirectory, archivoXML);
                c.IncludeXmlComments(rutaXML);
            });

            services.AddAutoMapper(typeof(Startup));
            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDBContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("IsAdmin", politica => politica.RequireClaim("isAdmin"));
            });

            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("https://www.apirequest.io").AllowAnyMethod().AllowAnyHeader()
                    .WithExposedHeaders(new string[] { "CantidadTotalRegistros" });
                });
            });

            services.AddDataProtection();

            services.AddTransient<HashService>();
            services.AddTransient<GeneradorEnlaces>();
            services.AddTransient<HATEOASAutorFilterAttribute>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //Aplication Insights
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = Configuration["ApplicationInsights:ConnectionString"];
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseLogResponseHttp();
            //app.UseMiddleware<LogResponseHttpMiddleware>();

            app.Map("/ruta1", app =>
            {
                app.Run(async context =>
                {
                    await context.Response.WriteAsync("I am intercepting the pipe");
                });
            });

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIAutores v1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "WebAPIAutores v2");

            });

            app.UseRouting();

            app.UseCors();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); }); 
        }
    }
}
