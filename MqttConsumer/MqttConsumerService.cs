using System.Text;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using MQTTnet.Packets;

public class MqttConsumerService : BackgroundService
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttConfigurator _mqttConfigurator;
    private readonly ILogger<MqttConsumerService> _logger;
    private readonly MqttClientConfigurationOptions _configuration;

    public MqttConsumerService(
        ILogger<MqttConsumerService> logger,
        IMqttClient mqttClient,
        MqttConfigurator mqttConfigurator,
        IOptions<MqttClientConfigurationOptions> configuration
    )
    {
        _mqttClient = mqttClient;
        _mqttConfigurator = mqttConfigurator;
        _logger = logger;
        _configuration = configuration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _mqttClient.ConnectedAsync += async e => await OnConnectedAsync(e);
        _mqttClient.DisconnectedAsync += async e => await OnDisconnectedAsync(e);
        _mqttClient.ApplicationMessageReceivedAsync += async e =>
            await OnApplicationMessageReceivedAsync(e);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!_mqttClient.IsConnected)
                {
                    _logger.LogInformation("Connecting to MQTT broker...");

                    await _mqttClient.ConnectAsync(_mqttConfigurator.ConfigureMqttClientOptions(_configuration), cancellationToken);

                    await _mqttClient.SubscribeAsync(
                        new MqttTopicFilter
                        {
                            Topic = _configuration.Topic,
                            QualityOfServiceLevel = _configuration.QoSLevel,
                        },
                        cancellationToken
                    );

                    _logger.LogInformation("Subscribed to topic: {topic}", _configuration.Topic);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error while connecting or subscribing to MQTT broker: {error}",
                    ex.Message
                );
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_configuration.StatusCheckDelaySeconds),
                cancellationToken
            );
        }

        // Clean up when stopping the service
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.UnsubscribeAsync(_configuration.Topic, cancellationToken);
            await _mqttClient.DisconnectAsync(
                MqttClientDisconnectOptionsReason.NormalDisconnection
            );
        }

        _mqttClient.Dispose();
        _logger.LogInformation("MQTT client disconnected and disposed");
    }

    private async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
    {
        _logger.LogInformation("MQTT client connected successfully");
    }

    private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        _logger.LogWarning(
            "MQTT client disconnected. Reason: {reason}. Was connected: {wasConnected}",
            e.ReasonString,
            e.ClientWasConnected
        );
    }

    private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
        _logger.LogInformation(
            "Message received on topic {topic}: {payload}",
            e.ApplicationMessage.Topic,
            payload
        );
    }
}
