using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel(IRedisService redisService, IMessageQueueService messageQueueService) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string text, string country)
    {
        var id = Guid.NewGuid().ToString();

        var region = GetRegionByCountry(country);

        await redisService.SaveRegion(id, region);

        await redisService.SaveText(id, text, region);

        var similarity = redisService.CalculateSimilarity(id, text, region);
        await redisService.SaveSimilarity(id, similarity, region);

        await messageQueueService.PublishSimilarityCalculatedEventAsync(id, similarity);

        await messageQueueService.PublishMessageAsync("text_queue", id);

        return RedirectToPage("/Summary", new { id });
    }

    private string GetRegionByCountry(string country)
    {
        return country switch
        {
            "Russia" => "RU",
            "France" => "EU",
            "Germany" => "EU",
            "UAE" => "ASIA",
            "India" => "ASIA",
            _ => throw new ArgumentException($"Unknown country: {country}")
        };
    }
}