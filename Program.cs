using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TareasService.Common;
using TareasService.Common.Behaviors;
using TareasService.Infrastructure.Clients;
using TareasService.Infrastructure.Clients.Estadisticas;
using TareasService.Infrastructure.Clients.Grupos;
using TareasService.Infrastructure.Clients.Materias;
using TareasService.Infrastructure.Clients.Notificaciones;
using TareasService.Infrastructure.Eventos;

var builder = WebApplication.CreateBuilder(args);

// --- Entity Framework Core: Postgres + columnas en snake_case (usuario_id, fecha_entrega) ---
builder.Services.AddDbContext<TareasDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

// --- MediatR: Commands/Queries + validación automática antes de cada handler ---
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// --- FluentValidation (registra todos los validadores del proyecto) ---
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// --- Mapster (mapeo Domain <-> DTO) ---
builder.Services.AddMapster();

// --- Carter (descubre los endpoints de cada feature) ---
builder.Services.AddCarter();

// --- JSON como el contrato de tareas ---
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // tarea_id, fecha_entrega, hora_entrega...
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

    // Enums en texto: EnProgreso ⇄ "en_progreso", Alta ⇄ "alta" (al responder y al recibir)
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));

    // Horas como el contrato: "18:00" sin segundos (al responder y al recibir)
    options.SerializerOptions.Converters.Add(new HoraJsonConverter());
});

// --- JSON mal formado o enum inválido → excepción → GlobalExceptionHandler (400 con formato del contrato) ---
builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);

// --- Manejo global de errores: { exito: false, error: { codigo, tipo, mensaje } } ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// --- Para reenviar el token del usuario a materias y grupos (token relay) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenRelayHandler>();

// --- Clients que validan: token del usuario + Polly (reintentos, timeout, circuit breaker) ---
builder.Services.AddHttpClient<IMateriasClient, MateriasClient>(client =>
        client.BaseAddress = new Uri(builder.Configuration["Servicios:Materias"]
            ?? throw new InvalidOperationException("Falta configurar Servicios:Materias")))
    .AddHttpMessageHandler<TokenRelayHandler>()
    .AddStandardResilienceHandler();

builder.Services.AddHttpClient<IGruposClient, GruposClient>(client =>
        client.BaseAddress = new Uri(builder.Configuration["Servicios:Grupos"]
            ?? throw new InvalidOperationException("Falta configurar Servicios:Grupos")))
    .AddHttpMessageHandler<TokenRelayHandler>()
    .AddStandardResilienceHandler();

// --- Clients que avisan: endpoints internos (sin token) y tiempos cortos, nunca frenan la tarea ---
Action<HttpStandardResilienceOptions> tiemposCortos = options =>
{
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
};

builder.Services.AddHttpClient<INotificacionesClient, NotificacionesClient>(client =>
        client.BaseAddress = new Uri(builder.Configuration["Servicios:Notificaciones"]
            ?? throw new InvalidOperationException("Falta configurar Servicios:Notificaciones")))
    .AddStandardResilienceHandler(tiemposCortos);

builder.Services.AddHttpClient<IEstadisticasClient, EstadisticasClient>(client =>
        client.BaseAddress = new Uri(builder.Configuration["Servicios:Estadisticas"]
            ?? throw new InvalidOperationException("Falta configurar Servicios:Estadisticas")))
    .AddStandardResilienceHandler(tiemposCortos);



// --- Eventos de tareas: hoy HTTP a notificaciones + estadísticas, mañana RabbitMQ ---
builder.Services.AddScoped<IEventosTarea, EventosTarea>();

// --- JWT: valida los tokens que genera users-auth (HS256 + JWT_SECRET compartido) ---
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta configurar Jwt:Key");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Deja los claims con su nombre original: "sub" sigue siendo "sub"
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Lo que SÍ se valida
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateLifetime = true,

            // Auth no pone emisor ni audiencia en sus tokens
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
});

var app = builder.Build();

// --- Primero: captura cualquier excepción de lo que viene después ---
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Crea o actualiza las tablas al arrancar
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<TareasDbContext>().Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

// --- Endpoints: Carter registra todos los ICarterModule del proyecto ---
app.MapCarter();

app.Run();