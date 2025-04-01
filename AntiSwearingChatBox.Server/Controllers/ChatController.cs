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
    // DTO classes to prevent circular references
    public class ThreadDto
    {
        public int ThreadId { get; set; }
        public string? Title { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsActive { get; set; }
        public bool ModerationEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
    }
    
    public class ParticipantDto
    {
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public UserDto? User { get; set; }
    }
    
    public class UserDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ThreadId { get; set; }
        public int UserId { get; set; }
        public string? OriginalMessage { get; set; }
        public string? ModeratedMessage { get; set; }
        public bool WasModified { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto? User { get; set; }
    }
    
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
                
                // Map to DTOs to prevent circular references
                var threadDtos = threads.Select(t => new ThreadDto
                {
                    ThreadId = t.ThreadId,
                    Title = t.Title,
                    IsPrivate = t.IsPrivate,
                    IsActive = t.IsActive,
                    ModerationEnabled = t.ModerationEnabled,
                    CreatedAt = t.CreatedAt,
                    LastMessageAt = t.LastMessageAt
                }).ToList();
                
                return Ok(threadDtos);
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
                
                // Map to DTO to prevent circular references
                var threadDto = new ThreadDto
                {
                    ThreadId = thread.ThreadId,
                    Title = thread.Title,
                    IsPrivate = thread.IsPrivate,
                    IsActive = thread.IsActive,
                    ModerationEnabled = thread.ModerationEnabled,
                    CreatedAt = thread.CreatedAt,
                    LastMessageAt = thread.LastMessageAt
                };
                
                return Ok(threadDto);
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
                
                // Create DTO for response
                var threadDto = new ThreadDto
                {
                    ThreadId = chatThread.ThreadId,
                    Title = chatThread.Title,
                    IsPrivate = chatThread.IsPrivate,
                    IsActive = chatThread.IsActive,
                    ModerationEnabled = chatThread.ModerationEnabled,
                    CreatedAt = chatThread.CreatedAt,
                    LastMessageAt = chatThread.LastMessageAt
                };
                
                return Ok(new { 
                    Success = true, 
                    Message = "Thread created successfully",
                    Thread = threadDto
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
                
                // Enrich with user data and map to DTO
                var enrichedMessageDtos = messages.Select(msg => {
                    var user = _userService.GetById(msg.UserId);
                    return new MessageDto
                    {
                        MessageId = msg.MessageId,
                        ThreadId = msg.ThreadId,
                        UserId = msg.UserId,
                        OriginalMessage = msg.OriginalMessage,
                        ModeratedMessage = msg.ModeratedMessage,
                        WasModified = msg.WasModified,
                        CreatedAt = msg.CreatedAt,
                        User = user != null ? new UserDto
                        {
                            UserId = user.UserId,
                            Username = user.Username,
                            Email = user.Email,
                            Role = user.Role,
                            IsActive = user.IsActive
                        } : null
                    };
                }).ToList();
                
                return Ok(enrichedMessageDtos);
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
                // Validate that thread exists
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    return NotFound(new { Success = false, Message = "Thread not found" });
                }
                
                // Validate that user is a participant in the thread
                var participants = _threadParticipantService.GetByThreadId(threadId);
                if (!participants.Any(p => p.UserId == model.UserId))
                {
                    return BadRequest(new { Success = false, Message = "You are not a participant in this thread" });
                }
                
                // Filter profanity if moderation is enabled
                string originalMessage = model.Message;
                string moderatedMessage = originalMessage;
                bool wasModified = false;
                
                if (thread.ModerationEnabled)
                {
                    (moderatedMessage, wasModified) = _profanityFilter.FilterProfanity(originalMessage);
                }
                
                // Create message history record
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
                
                // Create message DTO for response
                var user = _userService.GetById(model.UserId);
                var messageDto = new MessageDto
                {
                    MessageId = messageHistory.MessageId,
                    ThreadId = messageHistory.ThreadId,
                    UserId = messageHistory.UserId,
                    OriginalMessage = messageHistory.OriginalMessage,
                    ModeratedMessage = messageHistory.ModeratedMessage,
                    WasModified = messageHistory.WasModified,
                    CreatedAt = messageHistory.CreatedAt,
                    User = user != null ? new UserDto
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        IsActive = user.IsActive
                    } : null
                };
                
                return Ok(new { 
                    Success = true, 
                    Message = "Message sent successfully",
                    MessageHistory = messageDto,
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
                
                // Enrich with user data and map to DTO
                var enrichedParticipantDtos = participants.Select(p => {
                    var user = _userService.GetById(p.UserId);
                    return new ParticipantDto
                    {
                        UserId = p.UserId,
                        JoinedAt = p.JoinedAt,
                        User = user != null ? new UserDto
                        {
                            UserId = user.UserId,
                            Username = user.Username,
                            Email = user.Email,
                            Role = user.Role,
                            IsActive = user.IsActive
                        } : null
                    };
                }).ToList();
                
                return Ok(enrichedParticipantDtos);
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
                // Get all threads the user participates in
                var userThreads = _threadParticipantService.GetByUserId(userId);
                var otherUserThreads = _threadParticipantService.GetByUserId(otherUserId);
                
                // Find common threads (threads both users participate in)
                var commonThreadIds = userThreads.Select(t => t.ThreadId)
                    .Intersect(otherUserThreads.Select(t => t.ThreadId))
                    .ToList();
                
                // Check for private chats among common threads
                foreach (var threadId in commonThreadIds)
                {
                    var thread = _chatThreadService.GetById(threadId);
                    if (thread != null && thread.IsPrivate && thread.IsActive)
                    {
                        // Check if this is a 2-person chat
                        var participants = _threadParticipantService.GetByThreadId(threadId);
                        if (participants.Count() == 2)
                        {
                            // Map to DTO to prevent circular references
                            var threadDto = new ThreadDto
                            {
                                ThreadId = thread.ThreadId,
                                Title = thread.Title,
                                IsPrivate = thread.IsPrivate,
                                IsActive = thread.IsActive,
                                ModerationEnabled = thread.ModerationEnabled,
                                CreatedAt = thread.CreatedAt,
                                LastMessageAt = thread.LastMessageAt
                            };
                            
                            return Ok(new { 
                                Success = true, 
                                Found = true,
                                Thread = threadDto
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

        [HttpGet("find-by-name")]
        public IActionResult FindThreadByName([FromQuery] string name, [FromQuery] int userId)
        {
            try
            {
                // Get all threads the user participates in
                var userParticipations = _threadParticipantService.GetByUserId(userId);
                var userThreadIds = userParticipations.Select(p => p.ThreadId).ToList();
                
                // Find threads matching the name (case-insensitive)
                var matchingThreads = _chatThreadService.GetAll()
                    .Where(t => t.Title.ToLower().Contains(name.ToLower()) && userThreadIds.Contains(t.ThreadId))
                    .ToList();
                
                if (matchingThreads.Any())
                {
                    // Map to DTOs to prevent circular references
                    var threadDtos = matchingThreads.Select(t => new ThreadDto
                    {
                        ThreadId = t.ThreadId,
                        Title = t.Title,
                        IsPrivate = t.IsPrivate,
                        IsActive = t.IsActive,
                        ModerationEnabled = t.ModerationEnabled,
                        CreatedAt = t.CreatedAt,
                        LastMessageAt = t.LastMessageAt
                    }).ToList();
                    
                    return Ok(new { 
                        Success = true, 
                        Found = true,
                        Threads = threadDtos
                    });
                }
                
                // No matching threads found
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