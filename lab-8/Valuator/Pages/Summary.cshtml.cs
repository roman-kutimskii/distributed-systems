using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Valuator.Services;

namespace Valuator.Pages;

public class SummaryModel(IRedisService redisService, ILogger<SummaryModel> logger) : PageModel
{
    public double? Rank { get; private set; }
    public double Similarity { get; private set; }

    public async Task<IActionResult> OnGet(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Login");

        var authorId = await redisService.GetTextAuthor(id);
        if (authorId != userId) return RedirectToPage("/AccessDenied");

        await TryGetData(id);
        return Page();
    }

    public async Task<JsonResult> OnGetCheckData(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

        var authorId = await redisService.GetTextAuthor(id);
        if (authorId != userId) return new JsonResult(new { error = "Access denied" }) { StatusCode = 403 };

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
            var region = await redisService.GetRegionForId(id);


            var regionalDb = redisService.GetRegionalDb(region);

            var rankValue = await regionalDb.StringGetAsync("RANK-" + id);
            if (rankValue.HasValue && double.TryParse(rankValue.ToString(), out var rank)) Rank = rank;

            var similarityValue = await regionalDb.StringGetAsync("SIMILARITY-" + id);
            if (similarityValue.HasValue && double.TryParse(similarityValue.ToString(), out var similarity))
                Similarity = similarity;

            logger.LogInformation($"LOOKUP: {id}, {region}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error retrieving data for ID {id}");
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