using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RiskLens.Ai;
using RiskLens.App.ViewModels;
using RiskLens.Data;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Net.Http;

namespace RiskLens.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 数据库放在 %AppData%\RiskLens，数据不出本机
            var dbDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RiskLens");
            Directory.CreateDirectory(dbDir);
            var dbPath = Path.Combine(dbDir, "risklens.db");

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddDbContext<RiskLensDbContext>(o =>
                        o.UseSqlite($"Data Source={dbPath}"));

                    services.AddSingleton<IExtractionService>(sp => new OllamaExtractionService(
                        new HttpClient { Timeout = TimeSpan.FromMinutes(3) },
                        ctx.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434",
                        ctx.Configuration["Ollama:Model"] ?? "qwen2.5:7b"));

                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<MainWindow>();
                })
                .Build();

            // EnsureCreated：首次运行自动建库建表
            using var scope = _host.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<RiskLensDbContext>()
                .Database.EnsureCreatedAsync();

            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host is not null) await _host.StopAsync();
            base.OnExit(e);
        }
    }

}
