using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttConsumer.Options
{
    public class MqttCertificatesOptions
    {
        [Required]
        public string ClientCertificate { get; init; } = string.Empty;

        [Required]
        public string ClientKey { get; init; } = string.Empty;

        [DefaultValue("")]
        public string? ClientCertPassword { get; init; } = string.Empty;

        [DefaultValue("")]
        public string? CaCert { get; init; } = string.Empty;
        
    }
}
