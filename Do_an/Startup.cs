using Do_an.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using System;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;

[assembly: OwinStartup(typeof(Do_an.Startup))]

namespace Do_an
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var cauHinhKetNoi =
                ConfigurationManager.ConnectionStrings[
                    "SPORTIXEntities"];

            if (cauHinhKetNoi == null)
            {
                throw new ConfigurationErrorsException(
                    "Không tìm thấy kết nối SPORTIXEntities.");
            }

            var ketNoiEf = new EntityConnectionStringBuilder(
                cauHinhKetNoi.ConnectionString);

            Hangfire.GlobalConfiguration.Configuration
                .UseSqlServerStorage(
                    ketNoiEf.ProviderConnectionString,
                    new SqlServerStorageOptions
                    {
                        SqlClientFactory = SqlClientFactory.Instance,
                        PrepareSchemaIfNecessary = true
                    });

            RecurringJob.AddOrUpdate<CanhBaoTonKhoService>(
                "sportix-kiem-tra-ton-kho",
                job => job.KiemTra(),
                "* * * * *",
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Utc
                });

            app.UseHangfireServer(
                new BackgroundJobServerOptions
                {
                    WorkerCount = 1
                });

            RecurringJob.TriggerJob(
                "sportix-kiem-tra-ton-kho");
        }
    }
}