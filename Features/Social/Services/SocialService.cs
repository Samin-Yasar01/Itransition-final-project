using FormsApp.Data;
using FormsApp.Features.Social.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormsApp.Features.Social.Services
{
    public class SocialService : ISocialService
    {
        private readonly ApplicationDbContext _context;

        public SocialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCommentAsync(Guid templateId, string userId, string content)
        {
            var comment = new Comment
            {
                TemplateId = templateId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleLikeAsync(Guid templateId, string userId)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.TemplateId == templateId && l.UserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                _context.Likes.Add(new Like
                {
                    TemplateId = templateId,
                    UserId = userId
                });
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetCommentsAsync(Guid templateId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.TemplateId == templateId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetLikeCountAsync(Guid templateId)
        {
            return await _context.Likes
                .CountAsync(l => l.TemplateId == templateId);
        }

        public async Task<bool> HasLikedAsync(Guid templateId, string userId)
        {
            return await _context.Likes
                .AnyAsync(l => l.TemplateId == templateId && l.UserId == userId);
        }

        public async Task<List<Comment>> GetCommentsForAnswerAsync(Guid answerId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.AnswerId == answerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment> AddCommentToAnswerAsync(Guid answerId, string userId, string content)
        {
            var comment = new Comment
            {
                AnswerId = answerId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return await _context.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == comment.Id);
        }

        public async Task<(int likeCount, bool hasLiked)> ToggleLikeForAnswerAsync(Guid answerId, string userId)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.AnswerId == answerId && l.UserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                _context.Likes.Add(new Like
                {
                    AnswerId = answerId,
                    UserId = userId
                });
            }
            await _context.SaveChangesAsync();

            var likeCount = await _context.Likes.CountAsync(l => l.AnswerId == answerId);
            var hasLiked = existingLike == null; // If we removed a like, hasLiked is false

            return (likeCount, hasLiked);
        }

        public async Task<int> GetLikeCountForAnswerAsync(Guid answerId)
        {
            return await _context.Likes.CountAsync(l => l.AnswerId == answerId);
        }

        public async Task<bool> HasLikedAnswerAsync(Guid answerId, string userId)
        {
            return await _context.Likes.AnyAsync(l => l.AnswerId == answerId && l.UserId == userId);
        }
    }
}