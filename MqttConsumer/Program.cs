using Microsoft.Extensions.Options;
using MqttConsumer.Options;
using MQTTnet;
using MQTTnet.Client;

namespace MqttConsumer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = Host.CreateApplicationBuilder(args);

            builder.Configuration.AddConfiguration(configuration);

            builder.Services.AddOptions<CertificateConfigurationOptions>()
                .BindConfiguration("MqttClientCertificates")
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddOptions<MqttClientConfigurationOptions>()
                .BindConfiguration("MqttClientConfiguration")
                .ValidateDataAnnotations()
                .ValidateOnStart();
            
            builder.Services.AddSingleton<IMqttClient>(factory => new MqttFactory().CreateMqttClient());
            builder.Services.AddSingleton<CertificateConfigurator>();
            builder.Services.AddSingleton<MqttConfigurator>();

            builder.Services.AddHostedService<MqttConsumerService>();

            var host = builder.Build();
            host.Run();
        }
    }

}