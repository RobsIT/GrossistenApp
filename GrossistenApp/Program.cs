using GrossistenApp.Service;

namespace GrossistenApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient("MyCallApiService", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7240/api/");
            }
);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddScoped<ICallApiService, CallApiService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
