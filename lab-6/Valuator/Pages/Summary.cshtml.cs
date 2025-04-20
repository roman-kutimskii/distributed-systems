using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Valuator.Services;

namespace Valuator.Pages;

public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IRedisService _redisService;

    public SummaryModel(IRedisService redisService, ILogger<SummaryModel> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    public double? Rank { get; private set; }
    public double Similarity { get; private set; }

    public async Task OnGet(string id)
    {
        await TryGetData(id);
    }

    public async Task<JsonResult> OnGetCheckData(string id)
    {
        await TryGetData(id);

        return new JsonResult(new
        {
            rank = Rank,
            similarity = Similarity
        });
    }

    private async Task TryGetData(string id)
    {
        try
        {
            var region = await _redisService.GetRegionForId(id);


            var regionalDb = _redisService.GetRegionalDb(region);

            var rankValue = await regionalDb.StringGetAsync("RANK-" + id);
            if (rankValue.HasValue && double.TryParse(rankValue.ToString(), out var rank)) Rank = rank;

            var similarityValue = await regionalDb.StringGetAsync("SIMILARITY-" + id);
            if (similarityValue.HasValue && double.TryParse(similarityValue.ToString(), out var similarity))
                Similarity = similarity;

            _logger.LogInformation($"LOOKUP: {id}, {region}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving data for ID {id}");
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