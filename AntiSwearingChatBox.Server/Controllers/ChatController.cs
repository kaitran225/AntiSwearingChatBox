using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AntiSwearingChatBox.Service.Interface;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.AI.Interfaces;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatThreadService _chatThreadService;
        private readonly IMessageHistoryService _messageHistoryService;
        private readonly IThreadParticipantService _threadParticipantService;
        private readonly IUserService _userService;
        private readonly IProfanityFilter _profanityFilter;

        public ChatController(
            IChatThreadService chatThreadService,
            IMessageHistoryService messageHistoryService,
            IThreadParticipantService threadParticipantService,
            IUserService userService,
            IProfanityFilter profanityFilter)
        {
            _chatThreadService = chatThreadService;
            _messageHistoryService = messageHistoryService;
            _threadParticipantService = threadParticipantService;
            _userService = userService;
            _profanityFilter = profanityFilter;
        }

        [HttpGet("threads")]
        public IActionResult GetThreads([FromQuery] int userId)
        {
            try
            {
                var participations = _threadParticipantService.GetByUserId(userId);
                var threadIds = participations.Select(p => p.ThreadId).ToList();
                
                var threads = _chatThreadService.GetAll()
                    .Where(t => threadIds.Contains(t.ThreadId))
                    .ToList();
                
                return Ok(threads);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("threads/{threadId}")]
        public IActionResult GetThreadById(int threadId)
        {
            try
            {
                var thread = _chatThreadService.GetById(threadId);
                
                if (thread == null)
                {
                    return NotFound(new { Success = false, Message = "Thread not found" });
                }
                
                return Ok(thread);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("threads")]
        public IActionResult CreateThread([FromBody] CreateThreadModel model)
        {
            try
            {
                var chatThread = new ChatThread
                {
                    Title = model.Title,
                    IsPrivate = model.IsPrivate,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true,
                    ModerationEnabled = true
                };
                
                var result = _chatThreadService.Add(chatThread);
                
                if (!result.success)
                {
                    return BadRequest(new { Success = false, Message = result.message });
                }
                
                // Add creator as participant
                var creatorParticipant = new ThreadParticipant
                {
                    ThreadId = chatThread.ThreadId,
                    UserId = model.CreatorUserId,
                    JoinedAt = DateTime.UtcNow
                };
                
                _threadParticipantService.Add(creatorParticipant);
                
                // If this is a personal chat, add the other user too
                if (model.IsPrivate && model.OtherUserId.HasValue)
                {
                    var otherUserParticipant = new ThreadParticipant
                    {
                        ThreadId = chatThread.ThreadId,
                        UserId = model.OtherUserId.Value,
                        JoinedAt = DateTime.UtcNow
                    };
                    
                    _threadParticipantService.Add(otherUserParticipant);
                }
                
                return Ok(new { 
                    Success = true, 
                    Message = "Thread created successfully",
                    Thread = chatThread
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("threads/{threadId}/messages")]
        public IActionResult GetMessages(int threadId)
        {
            try
            {
                var messages = _messageHistoryService.GetByThreadId(threadId).ToList();
                
                // Enrich with user data
                var enrichedMessages = messages.Select(msg => new 
                {
                    Message = msg,
                    User = _userService.GetById(msg.UserId)
                }).ToList();
                
                return Ok(enrichedMessages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("threads/{threadId}/messages")]
        public IActionResult SendMessage(int threadId, [FromBody] SendMessageModel model)
        {
            try
            {
                // Verify thread exists
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    return NotFound(new { Success = false, Message = "Thread not found" });
                }
                
                // Verify user is a participant
                var participants = _threadParticipantService.GetByThreadId(threadId);
                if (!participants.Any(p => p.UserId == model.UserId))
                {
                    return BadRequest(new { Success = false, Message = "User is not a member of this thread" });
                }
                
                // Check if we need to moderate
                string originalMessage = model.Message;
                string moderatedMessage = originalMessage;
                bool wasModified = false;
                
                if (thread.ModerationEnabled)
                {
                    (moderatedMessage, wasModified) = _profanityFilter.FilterProfanity(originalMessage);
                }
                
                // Create and save the message
                var messageHistory = new MessageHistory
                {
                    ThreadId = threadId,
                    UserId = model.UserId,
                    OriginalMessage = originalMessage,
                    ModeratedMessage = moderatedMessage,
                    WasModified = wasModified,
                    CreatedAt = DateTime.UtcNow
                };
                
                var result = _messageHistoryService.Add(messageHistory);
                
                if (!result.success)
                {
                    return BadRequest(new { Success = false, Message = result.message });
                }
                
                // Update the thread's LastMessageAt timestamp
                thread.LastMessageAt = DateTime.UtcNow;
                _chatThreadService.Update(thread);
                
                return Ok(new { 
                    Success = true, 
                    Message = "Message sent successfully",
                    MessageHistory = messageHistory,
                    WasModerated = wasModified
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("threads/{threadId}/participants")]
        public IActionResult GetParticipants(int threadId)
        {
            try
            {
                var participants = _threadParticipantService.GetByThreadId(threadId);
                
                // Enrich with user data
                var enrichedParticipants = participants.Select(p => new 
                {
                    Participant = p,
                    User = _userService.GetById(p.UserId)
                }).ToList();
                
                return Ok(enrichedParticipants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("threads/{threadId}/participants")]
        public IActionResult AddParticipant(int threadId, [FromBody] ParticipantModel model)
        {
            try
            {
                // Verify thread exists
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    return NotFound(new { Success = false, Message = "Thread not found" });
                }
                
                // Check if this is a personal chat (private chat between 2 users)
                if (thread.IsPrivate)
                {
                    var participants = _threadParticipantService.GetByThreadId(threadId);
                    if (participants.Count() == 2)
                    {
                        return BadRequest(new { Success = false, Message = "Cannot add users to a personal chat" });
                    }
                }
                
                // Verify the requester is a participant and is the creator
                var existingParticipants = _threadParticipantService.GetByThreadId(threadId);
                
                if (!existingParticipants.Any(p => p.UserId == model.RequestedByUserId))
                {
                    return BadRequest(new { Success = false, Message = "Requester is not a member of this thread" });
                }
                
                // Verify requester is the creator (first participant)
                var firstParticipant = existingParticipants.OrderBy(p => p.JoinedAt).FirstOrDefault();
                if (firstParticipant == null || firstParticipant.UserId != model.RequestedByUserId)
                {
                    return BadRequest(new { Success = false, Message = "Only the thread creator can add members" });
                }
                
                // Verify user to add exists
                var userToAdd = _userService.GetById(model.UserId);
                if (userToAdd == null)
                {
                    return NotFound(new { Success = false, Message = "User to add not found" });
                }
                
                // Check if already a member
                if (existingParticipants.Any(p => p.UserId == model.UserId))
                {
                    return BadRequest(new { Success = false, Message = "User is already a member of this thread" });
                }
                
                // Add the new participant
                var participant = new ThreadParticipant
                {
                    ThreadId = threadId,
                    UserId = model.UserId,
                    JoinedAt = DateTime.UtcNow
                };
                
                var result = _threadParticipantService.Add(participant);
                
                if (!result.success)
                {
                    return BadRequest(new { Success = false, Message = result.message });
                }
                
                return Ok(new { 
                    Success = true, 
                    Message = $"User added to thread successfully",
                    Participant = participant
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("threads/{threadId}/participants/{userId}")]
        public IActionResult RemoveParticipant(int threadId, int userId, [FromQuery] int requestedByUserId)
        {
            try
            {
                // Verify thread exists
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    return NotFound(new { Success = false, Message = "Thread not found" });
                }
                
                // Get all participants
                var participants = _threadParticipantService.GetByThreadId(threadId);
                
                // Verify requester is a member
                if (!participants.Any(p => p.UserId == requestedByUserId))
                {
                    return BadRequest(new { Success = false, Message = "Requester is not a member of this thread" });
                }
                
                // Check permissions: can remove self, or if creator can remove others
                var firstParticipant = participants.OrderBy(p => p.JoinedAt).FirstOrDefault();
                var isCreator = firstParticipant != null && firstParticipant.UserId == requestedByUserId;
                
                if (requestedByUserId != userId && !isCreator)
                {
                    return BadRequest(new { Success = false, Message = "Only the thread creator can remove other members" });
                }
                
                // Verify user to remove exists in the thread
                if (!participants.Any(p => p.UserId == userId))
                {
                    return BadRequest(new { Success = false, Message = "User is not a member of this thread" });
                }
                
                // Remove the participant
                var result = _threadParticipantService.RemoveUserFromThread(userId, threadId);
                
                if (!result)
                {
                    return BadRequest(new { Success = false, Message = "Failed to remove user from thread" });
                }
                
                return Ok(new { Success = true, Message = "User removed from thread successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("personal-chat")]
        public IActionResult FindPersonalChat([FromQuery] int userId, [FromQuery] int otherUserId)
        {
            try
            {
                // Get threads where both users are participants
                var userThreads = _threadParticipantService.GetByUserId(userId);
                var otherUserThreads = _threadParticipantService.GetByUserId(otherUserId);
                
                // Get the intersection of thread IDs
                var sharedThreadIds = userThreads.Select(t => t.ThreadId)
                    .Intersect(otherUserThreads.Select(t => t.ThreadId))
                    .ToList();
                
                // Find personal chats among shared threads
                foreach (var threadId in sharedThreadIds)
                {
                    var thread = _chatThreadService.GetById(threadId);
                    if (thread != null && thread.IsPrivate && thread.IsActive)
                    {
                        // Check if this is a 2-person chat
                        var participants = _threadParticipantService.GetByThreadId(threadId);
                        if (participants.Count() == 2)
                        {
                            return Ok(new { 
                                Success = true, 
                                Found = true,
                                Thread = thread
                            });
                        }
                    }
                }
                
                // No existing personal chat found
                return Ok(new { Success = true, Found = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }
    }

    public class CreateThreadModel
    {
        public string Title { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
        public int CreatorUserId { get; set; }
        public int? OtherUserId { get; set; } // For personal chats
    }

    public class SendMessageModel
    {
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ParticipantModel
    {
        public int UserId { get; set; }
        public int RequestedByUserId { get; set; }
    }
} 