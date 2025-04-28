// Features/Social/Controllers/SocialController.cs

using System.Security.Claims;
using FormsApp.Features.Social.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormsApp.Features.Social.Controllers
{
    [Authorize]
    public class SocialController : Controller
    {
        private readonly ISocialService _socialService;

        public SocialController(ISocialService socialService)
        {
            _socialService = socialService;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Guid templateId, string content)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException();

            await _socialService.AddCommentAsync(templateId, userId, content);
            
            // Get the newly added comment with user info
            var comments = await _socialService.GetCommentsAsync(templateId);
            var newComment = comments.FirstOrDefault(c => c.UserId == userId && c.Content == content);
            
            if (newComment != null)
            {
                return Json(new
                {
                    content = newComment.Content,
                    userName = newComment.User?.FirstName + " " + newComment.User?.LastName,
                    createdAt = newComment.CreatedAt
                });
            }
            
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(Guid templateId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException();

            await _socialService.ToggleLikeAsync(templateId, userId);
            
            var likeCount = await _socialService.GetLikeCountAsync(templateId);
            var hasLiked = await _socialService.HasLikedAsync(templateId, userId);
            
            return Json(new
            {
                likeCount,
                hasLiked
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetComments(Guid answerId)
        {
            var comments = await _socialService.GetCommentsForAnswerAsync(answerId);
            return Json(comments.Select(c => new {
                content = c.Content,
                userName = c.User.FirstName + " " + c.User.LastName,
                createdAt = c.CreatedAt
            }));
        }

        [HttpPost]
        public async Task<IActionResult> AddCommentToAnswer(Guid answerId, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = await _socialService.AddCommentToAnswerAsync(answerId, userId, content);

            return Json(new
            {
                content = comment.Content,
                userName = comment.User.FirstName + " " + comment.User.LastName,
                createdAt = comment.CreatedAt
            });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLikeAnswer(Guid answerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _socialService.ToggleLikeForAnswerAsync(answerId, userId);

            return Json(new
            {
                likeCount = result.likeCount,
                hasLiked = result.hasLiked
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetLikeCount(Guid answerId)
        {
            var count = await _socialService.GetLikeCountForAnswerAsync(answerId);
            return Json(count);
        }

        [HttpGet]
        public async Task<IActionResult> HasLikedAnswer(Guid answerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasLiked = await _socialService.HasLikedAnswerAsync(answerId, userId);
            return Json(hasLiked);
        }
    }
}