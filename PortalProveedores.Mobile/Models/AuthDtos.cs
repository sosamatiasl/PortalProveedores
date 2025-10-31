using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PortalProveedores.Mobile.Models
{
    // DTO para enviar en el Login
    public class AuthRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;
    }

    // DTO que se recibe del Login
    public class AuthResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;

        [JsonProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("roles")]
        public List<string> Roles { get; set; } = new();

        [JsonProperty("errorMessage")]
        public string? ErrorMessage { get; set; }
    }
}
