using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel(IRedisService redisService, IMessageQueueService messageQueueService) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string text, string region)
    {
        var id = Guid.NewGuid().ToString();

        await redisService.SaveText(id, text, region);

        var similarity = redisService.CalculateSimilarity(id, text, region);
        await redisService.SaveSimilarity(id, similarity, region);

        await messageQueueService.PublishSimilarityCalculatedEventAsync(id, similarity);

        await messageQueueService.PublishMessageAsync("text_queue", id);

        return RedirectToPage("/Summary", new { id });
    }
}