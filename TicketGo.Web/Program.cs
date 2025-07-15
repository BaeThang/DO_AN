using Microsoft.EntityFrameworkCore;
using TicketGo.Application.Interfaces;
using TicketGo.Application.Services;
using TicketGo.Domain.Interfaces;
using TicketGo.Infrastructure.Data;
using TicketGo.Infrastructure.Repositories;
using TicketGo.Web.Middleware;
using Resend;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// DB context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TicketGoConnection")));

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ITrainRepository, TrainRepository>();
builder.Services.AddScoped<ICoachRepository, CoachRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IOrderTicketRepository, OrderTicketRepository>();
builder.Services.AddScoped<ITrainRouteRepository, TrainRouteRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();

// Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITrainService, TrainService>();
builder.Services.AddScoped<ICoachService, CoachService>();
builder.Services.AddScoped<ITrainRouteService, TrainRouteService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IResendService, ResendService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IVNPayService, VNPayService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Resend 

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>( o =>
{
    o.ApiToken ="re_NzRQ5Ptb_3V25XUff9ueWmDTLaGF5NQJB";
} );
builder.Services.AddTransient<IResend, ResendClient>();

// Add AutoMapper
builder.Services.AddHttpContextAccessor();

// Cookie Authentication
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Access/Login";
        options.AccessDeniedPath = "/Error/403";
         options.Events.OnRedirectToLogin = context =>
        {
            var requestPath = context.Request.Path;

            // Nếu là API hoặc đường dẫn không tồn tại, không redirect mà trả về 404
            if (context.Response.StatusCode == 404 || requestPath.StartsWithSegments("/"))
            {
                context.Response.Redirect("/Error/404");
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseSession();
app.UseAuthentication(); 
app.UseAuthorization();
// Area Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=TrangChu}/{id?}");

app.Run();
