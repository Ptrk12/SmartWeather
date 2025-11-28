using Interfaces.Repositories;
using Interfaces.Repositories.firebase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models.mqtt;
using MQTTnet;
using MQTTnet.Formatter;
using System.Text.Json;

namespace Managers.workers
{
    public class MqttBackgroundWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private ILogger<MqttBackgroundWorker> _logger;
        private IMqttClient _mqttClient;
        private readonly IServiceProvider _serviceProvider;

        public MqttBackgroundWorker(IConfiguration configuration, ILogger<MqttBackgroundWorker> logger, IMqttClient mqttClient, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _mqttClient = mqttClient;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttFactory = new MqttClientFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_configuration["MQTT:MQTT_SERVER"], int.Parse(_configuration["MQTT:MQTT_PORT"]))
                .WithCredentials(_configuration["MQTT:MQTT_USER"], _configuration["MQTT:MQTT_PASS"])
                .WithProtocolVersion(MqttProtocolVersion.V311).WithTlsOptions(new MqttClientTlsOptions
                {
                    UseTls = true,
                    AllowUntrustedCertificates = true,
                    IgnoreCertificateChainErrors = true,
                    IgnoreCertificateRevocationErrors = true
                })
                .Build();

            _mqttClient.ConnectedAsync += async e =>
            {
                _logger.LogInformation("Connected to MQTT");
                await _mqttClient.SubscribeAsync(_configuration["MQTT:MQTT_TOPIC"]);
            };

            await _mqttClient.ConnectAsync(options, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }

        private async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs msg)
        {
            try
            {
                var payload = msg.ApplicationMessage.ConvertPayloadToString();

                var message = JsonSerializer.Deserialize<MqttMessage>(payload,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (message != null)
                    await ProcessMessage(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT error");
            }
        }


        private async Task ProcessMessage(MqttMessage data)
        {
            using var scope = _serviceProvider.CreateScope();
            var firebaseRepo = scope.ServiceProvider.GetRequiredService<IFirebaseRepository>();
            var deviceRepo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

            string serialNumber = data.SerialNumber;
            long timestamp = Convert.ToInt64(data.Timestamp);
            var date =  DateTimeOffset.FromUnixTimeSeconds(timestamp);

            await firebaseRepo.PushToFirestore(serialNumber, data);
            await deviceRepo.UpdateDeviceLastMeasurement(serialNumber, date);
           // await InsertAlertLog(serialNumber, data);
        }
    }
}
