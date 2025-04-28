using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FormsApp.Features.Social.Models;

namespace FormsApp.Features.Social.Services
{
    public interface ISocialService
    {
        Task AddCommentAsync(Guid templateId, string userId, string content);
        Task ToggleLikeAsync(Guid templateId, string userId);
        Task<List<Comment>> GetCommentsAsync(Guid templateId);
        Task<int> GetLikeCountAsync(Guid templateId);
        Task<bool> HasLikedAsync(Guid templateId, string userId);
        Task<List<Comment>> GetCommentsForAnswerAsync(Guid answerId);
        Task<Comment> AddCommentToAnswerAsync(Guid answerId, string userId, string content);
        Task<(int likeCount, bool hasLiked)> ToggleLikeForAnswerAsync(Guid answerId, string userId);
        Task<int> GetLikeCountForAnswerAsync(Guid answerId);
        Task<bool> HasLikedAnswerAsync(Guid answerId, string userId);
    }
}