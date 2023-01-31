// See https://aka.ms/new-console-template for more information

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Bogus;

using Microsoft.IdentityModel.Tokens;

using Throw;

using static System.Console;

using static Microsoft.IdentityModel.Tokens.SecurityAlgorithms;

WriteLine("Testing Throw Validation Library.........");

try
{
    User user = new Faker<User>()
        .RuleFor(p => p.Id, p => p.Random.Guid())
        .RuleFor(p => p.Name, p => p.Person.FullName)
        .RuleFor(p => p.Email, p => p.Person.Email)
        .RuleFor(p => p.Password, p => p.Internet.Password())
        .RuleFor(p => p.Mobile, p => p.Person.Phone)
        .RuleFor(p => p.QId, p => RandomQId(11))
        .Generate();

    TokenProvider provider = new();
    string token = provider.GenerateToken(user);
    WriteLine(token);

    var claims = provider.ValidateToken(token).Principal;

    foreach (var items in claims!.Claims)
    {
        WriteLine($"{items.Type} : {items.Value}");
    }
    
    //generate only alphabetic letters with length 12

    static string RandomQId(int length) => new Faker().Internet.Random.AlphaNumeric(length).ToLower()
        .Replace("a", "1").Replace("b", "2").Replace("c", "3").Replace("d", "4").Replace("e", "5").Replace("f", "6")
        .Replace("g", "7").Replace("h", "8").Replace("i", "9").Replace("j", "0").Replace("k", "1").Replace("l", "2")
        .Replace("m", "3").Replace("n", "4").Replace("o", "5").Replace("p", "6").Replace("q", "7").Replace("r", "8")
        .Replace("s", "9").Replace("t", "0").Replace("u", "1").Replace("v", "2").Replace("w", "3").Replace("x", "4")
        .Replace("y", "5").Replace("z", "6");

    ReadLine();
}
catch (Exception exception)
{
    WriteLine(exception);
    ReadLine();
}

public class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Mobile { get; set; }
    public string? QId { get; set; }
}

public class TokenProvider
{
    public string GenerateToken(User user)
    {
        TokenService tokenService = new();
        TokenResponse tokenResponse = tokenService.GetTokenAsync(user).Result;
        return tokenResponse.Token;
    }

    public AccessTokenResult ValidateToken(string token)
    {
        TokenService tokenService = new();
        return tokenService.ValidateToken(token);
    }
}

public class TokenService
{
    private static readonly SymmetricSecurityKey? key = new("This is a secret key"u8.ToArray());
    public async Task<TokenResponse> GetTokenAsync(User request)
    {
        return await GenerateTokensAndUpdateUser(request);
    }

    private Task<TokenResponse> GenerateTokensAndUpdateUser(User request)
    {
        string token = GenerateJwt(request);
        return Task.FromResult(new TokenResponse(token));
    }

    private string GenerateJwt(User request) =>
        GenerateEncryptedToken(GetClaims(request));

    private static IEnumerable<Claim>? GetClaims(User request)
    {
        try
        {
            var claims = new List<Claim>();
            request.Id.Throw().IfNotType<Guid>().ToString()
                .ThrowIfNull().IfEmpty(p => p, "Id is empty")
                .IfWhiteSpace(p => p, "Id contains only whitespace")
                .IfNullOrEmpty(p => p, "Id is null or empty");

            claims.Add(new("UserId", request.Id.ToString()));

            request.Name.ThrowIfNull().IfEmpty(p => p, "Name is empty")
                .IfWhiteSpace(p => p, "Name contains only whitespace")
                .IfNullOrEmpty(p => p, "Name is null or empty");

            claims.Add(new("Name", request.Name));

            request.Email.ThrowIfNull().IfEmpty(p => p, "Email is empty")
                .IfWhiteSpace(p => p, "Email contains only whitespace")
                .IfNullOrEmpty(p => p, "Email is null or empty");

            claims.Add(new("Email", request.Email));

            request.Mobile.ThrowIfNull().IfEmpty(p => p, "Mobile is empty")
                .IfWhiteSpace(p => p, "Mobile contains only whitespace")
                .IfNullOrEmpty(p => p, "Mobile is null or empty");

            claims.Add(new("Phone", request.Mobile));

            request.QId.ThrowIfNull().IfEmpty(p => p, "QId is empty")
                .IfWhiteSpace(p => p, "QId contains only whitespace")
                .IfNullOrEmpty(p => p, "QId is null or empty");

            claims.Add(new("Identifier", request.QId));
            return claims;
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message);
            return null;
        }
    }

    private string GenerateEncryptedToken(IEnumerable<Claim>? claims)
    {
        var credentials = new SigningCredentials(key, HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "https://phcc.gov.qa",
            audience: "phcc.gov.qa",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    /*
     public User? FetchClaims(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var tokenCredentialsModel = new User
            {
                QId = jwtToken.Claims.FirstOrDefault(c => c.Type == NameIdentifier)?.Value!,
                Email = jwtToken.Claims.FirstOrDefault(c => c.Type == Email)?.Value,
                Mobile = jwtToken.Claims.FirstOrDefault(c => c.Type == MobilePhone)?.Value,
                Name = jwtToken.Claims.FirstOrDefault(c => c.Type == Name)?.Value,
                Id = Guid.Parse(jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value!)
            };
            return tokenCredentialsModel;
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message);
            return null;
        }
    }
    */

    public AccessTokenResult ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            var tokenParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "https://phcc.gov.qa",
                ValidAudience = "phcc.gov.qa",
                IssuerSigningKey = key
            };

            var result = handler.ValidateToken(token, tokenParams, out _);
            return AccessTokenResult.Success(result);
        }
        catch (SecurityTokenExpiredException ex)
        {
            WriteLine(ex.Message);
            return AccessTokenResult.Expired();
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            WriteLine(ex.Message);
            return AccessTokenResult.Error(ex);
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            WriteLine(ex.Message);
            return AccessTokenResult.Error(ex);
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            WriteLine(ex.Message);
            return AccessTokenResult.Error(ex);
        }
        catch (SecurityTokenInvalidTypeException ex)
        {
            WriteLine(ex.Message);
            return AccessTokenResult.Error(ex);
        }
    }
}

public record TokenResponse(string Token);

public class AccessTokenResult
{
    private AccessTokenResult()
    {
    }

    public ClaimsPrincipal? Principal { get; private init; }
    public AccessTokenStatus? Status { get; private init; }
    public Exception? Exception { get; private init; }

    public static AccessTokenResult Success(ClaimsPrincipal principal)
    {
        return new AccessTokenResult
        {
            Principal = principal,
            Status = AccessTokenStatus.Valid
        };
    }

    public static AccessTokenResult Expired()
    {
        return new AccessTokenResult
        {
            Status = AccessTokenStatus.Expired
        };
    }

    public static AccessTokenResult Error(Exception ex)
    {
        return new AccessTokenResult
        {
            Status = AccessTokenStatus.Error,
            Exception = ex
        };
    }

    public static AccessTokenResult NoToken()
    {
        return new AccessTokenResult
        {
            Status = AccessTokenStatus.NoToken
        };
    }
}

public enum AccessTokenStatus
{
    Valid,
    Expired,
    Error,
    NoToken
}