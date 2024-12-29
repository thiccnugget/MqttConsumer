namespace MqttConsumer.Options
{
    public class CertificateConfigurationOptions
    {
        public bool UseSecretsManager { get; set; }
        public string? SecretsManagerType { get; set; } // Example: "aws", "azure"
        public MqttCertificatesOptions? CertificatesPaths { get; init; } = default!;
        public MqttCertificatesOptions? CertificatesKeys { get; init; } = default!;
    }
}
