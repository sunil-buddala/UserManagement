namespace Application.Common.Settings;

public class JwtSettings
{
    public string AccessTokenSecret { get; set; }
    public string RefreshTokenSecret { get; set; }
    public double AccessTokenExpirationMinutes { get; set; }
    public double RefreshTokenExpirationMinutes { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }

    public GoogleSettings Google { get; set; }
}

public class GoogleSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}