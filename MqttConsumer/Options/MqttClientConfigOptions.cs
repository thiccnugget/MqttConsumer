using MQTTnet.Protocol;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


public sealed record MqttCertificates
{
    public string ClientCertificate { get; init; } = string.Empty;
    public string ClientKey { get; init; } = string.Empty;
    public string? ClientCertPassword { get; init; } = string.Empty;
    public string? CaCert { get; init; } = string.Empty;
}

public sealed record MqttClientConfigurationOptions
{
    [Required]
    public string Host { get; init; } = string.Empty;

    [Required, Range(1, 65535)]
    public int Port { get; init; } = default!;

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string Topic { get; init; } = string.Empty;

    [Required, DefaultValue(MqttQualityOfServiceLevel.ExactlyOnce)]
    public MqttQualityOfServiceLevel QoSLevel { get; init; } = MqttQualityOfServiceLevel.ExactlyOnce;

    [Required]
    public int StatusCheckDelaySeconds { get; init; } = 10;
    public bool RequiresServerCaCertValidation { get; init; }  = false;

}

