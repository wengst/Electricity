using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ElectricityApp
{
    [Serializable]
    public class AppConfig
    {
        [JsonPropertyName("theme")]
        public Theme MyTheme { get; set; } = Theme.浅色;
    }
}
