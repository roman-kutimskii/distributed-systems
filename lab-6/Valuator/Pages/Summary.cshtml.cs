using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Valuator.Pages;

public class SummaryModel(IConnectionMultiplexer redis) : PageModel
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

    public double? Rank { get; private set; }
    public double Similarity { get; private set; }

    public void OnGet(string id)
    {
    }

    public JsonResult OnGetCheckData(string id)
    {
        TryGetData(id);

        return new JsonResult(new
        {
            rank = Rank,
            similarity = Similarity
        });
    }

    private void TryGetData(string id)
    {
        var rankValue = _redisDb.StringGet("RANK-" + id);
        if (rankValue.HasValue && double.TryParse(rankValue, out var rank))
        {
            Rank = rank;
        }

        var similarityValue = _redisDb.StringGet("SIMILARITY-" + id);
        if (similarityValue.HasValue && double.TryParse(similarityValue, out var similarity))
        {
            Similarity = similarity;
        }
    }

    public string GenerateJwtToken(Claim[]? claims = null)
    {
        var securityKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                "RjtScMGYVJhJr3n-NJy22dB1MLWajjRNADCRdZqqHEttbozYUY1ZXyoO8sNm11sU256byxmeB391v4Vn3vMoPg"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            signingCredentials: credentials,
            claims: claims ?? []
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}