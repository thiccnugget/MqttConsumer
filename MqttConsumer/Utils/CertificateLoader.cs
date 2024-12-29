using Microsoft.Extensions.Options;
using MqttConsumer.Options;

public class CertificateConfigurator
{
    private readonly CertificateConfigurationOptions _options;

    public CertificateConfigurator(IOptions<CertificateConfigurationOptions> options)
    {
        _options = options.Value;
        if(_options.CertificatesKeys is null && _options.CertificatesPaths is null)
        {
            throw new InvalidOperationException("No certificates paths or keys found in configuration.");
        }
    }

    public MqttCertificates LoadCertificates()
    {
        return _options.UseSecretsManager
            ? LoadCertificatesFromSecretsManager()
            : LoadCertificatesFromFiles();
    }

    private MqttCertificates LoadCertificatesFromFiles()
    {
        if (_options.CertificatesPaths is null)
        {
            throw new InvalidOperationException("Certificates paths are not configured.");
        }

        return new MqttCertificates
        {
            ClientCertificate = File.ReadAllText(_options.CertificatesPaths.ClientCertificate),
            ClientKey = File.ReadAllText(_options.CertificatesPaths.ClientKey),

            ClientCertPassword = File.Exists(_options.CertificatesPaths.ClientCertPassword)
                ? File.ReadAllText(_options.CertificatesPaths.ClientCertPassword)
                : string.Empty,

            CaCert = File.Exists(_options.CertificatesPaths.CaCert)
                ? File.ReadAllText(_options.CertificatesPaths.CaCert)
                : string.Empty
        };

    }

    private MqttCertificates LoadCertificatesFromSecretsManager()
    {
        if (_options.CertificatesKeys is null)
        {
            throw new InvalidOperationException("Certificates keys are not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.SecretsManagerType))
        {
            throw new InvalidOperationException("Secrets manager type is not specified.");
        }

        return _options.SecretsManagerType switch
        {
            "aws" => LoadFromAWSSecretsManager(),
            "azure" => LoadFromAzureKeyVault(),
            _ => throw new NotSupportedException($"Secrets manager '{_options.SecretsManagerType}' is not supported yet.")
        };

    }

    private MqttCertificates LoadFromAWSSecretsManager()
    {
        throw new NotImplementedException("AWS Secrets Manager integration is not implemented.");
    }

    private MqttCertificates LoadFromAzureKeyVault()
    {
        throw new NotImplementedException("Azure Key Vault integration is not implemented.");
    }
}
