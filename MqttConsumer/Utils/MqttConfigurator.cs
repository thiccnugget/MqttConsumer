using System.Security.Cryptography.X509Certificates;
using System.Text;
using MQTTnet.Client;

public class MqttConfigurator
{
    private readonly CertificateConfigurator _certificateConfigurator;

    public MqttConfigurator(CertificateConfigurator certificateConfigurator)
    {
        _certificateConfigurator = certificateConfigurator;
    }

    public MqttClientOptions ConfigureMqttClientOptions(MqttClientConfigurationOptions options)
    {
        MqttCertificates certificates = _certificateConfigurator.LoadCertificates();

        if (
            options.RequiresServerCaCertValidation
            && string.IsNullOrWhiteSpace(certificates.CaCert)
        )
        {
            throw new ArgumentNullException(
                nameof(certificates.CaCert),
                """
                ServerCaCertificate must be a valid path to a Certificate Authority (CA) certificate when RequiresServerCaCertValidation is enabled.
                Either disable RequiresServerCaCertValidation or load a valid CA certificate.
                """
            );
        }

        X509Certificate2Collection clientCerts = new X509Certificate2Collection();

        // Load client certificate and key
        try
        {
            // Combine Client Certificate and Client Key in a single PFX file
            X509Certificate2 clientCert = string.IsNullOrEmpty(certificates.ClientCertPassword)
                ? X509Certificate2.CreateFromPem(
                    certPem: certificates.ClientCertificate,
                    keyPem: certificates.ClientKey
                )
                : X509Certificate2.CreateFromEncryptedPem(
                    certPem: certificates.ClientCertificate,
                    password: certificates.ClientCertPassword,
                    keyPem: certificates.ClientKey
                );

            clientCerts.Add(
                new X509Certificate2(clientCert.Export(X509ContentType.Pfx))
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "An error occurred while loading the client certificate and key.",
                ex
            );
        }

        MqttClientTlsOptions tlsParams = new MqttClientTlsOptions
        {
            UseTls = true,
            ClientCertificatesProvider = new DefaultMqttCertificatesProvider(clientCerts),
        };

        if (
            options.RequiresServerCaCertValidation
            && !string.IsNullOrWhiteSpace(certificates.CaCert)
        )
        {
            X509Certificate2 caCert;

            try
            {
                caCert = X509CertificateLoader.LoadCertificate(
                    Encoding.UTF8.GetBytes(certificates.CaCert)
                );
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(
                    "The provided CaCert is not a valid Certificate Authority certificate.",
                    nameof(certificates.CaCert),
                    ex
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "An error occurred while loading the Server CA certificate.",
                    ex
                );
            }

            // Trust the given CA cert
            tlsParams.CertificateValidationHandler = certContext =>
            {
                using var chain = new X509Chain
                {
                    ChainPolicy =
                    {
                        RevocationMode = X509RevocationMode.NoCheck,
                        RevocationFlag = X509RevocationFlag.ExcludeRoot,
                        VerificationFlags = X509VerificationFlags.NoFlag,
                        VerificationTime = DateTime.Now,
                        TrustMode = X509ChainTrustMode.CustomRootTrust,
                    },
                };

                chain.ChainPolicy.CustomTrustStore.Add(caCert);
                return chain.Build(new X509Certificate2(certContext.Certificate));
            };
        }
        else
        {
            // If no validation on the CA cert is required, trust all certificates
            // This is not recommended for production environments!
            tlsParams.CertificateValidationHandler = _ => true;
        }

        MqttClientOptionsBuilder mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(options.Host, options.Port)
            .WithClientId(options.ClientId)
            .WithTlsOptions(tlsParams);

        return mqttClientOptions.Build();
    }
}
