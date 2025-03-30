using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.Server.Controllers
{
    [ApiController]
    [Route("api/groups")]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IChatThreadService _chatThreadService;
        private readonly IThreadParticipantService _threadParticipantService;
        private readonly IUserService _userService;

        public GroupController(
            IChatThreadService chatThreadService,
            IThreadParticipantService threadParticipantService,
            IUserService userService)
        {
            _chatThreadService = chatThreadService;
            _threadParticipantService = threadParticipantService;
            _userService = userService;
        }

        [HttpPost("create")]
        public IActionResult CreateGroup([FromBody] CreateGroupRequest request)
        {
            if (string.IsNullOrEmpty(request.GroupName))
                return BadRequest(new { message = "Group name is required" });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Create a new thread for this group
            var chatThread = new ChatThread
            {
                Title = request.GroupName,
                IsPrivate = false,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow,
                IsActive = true,
                ModerationEnabled = true
            };

            var result = _chatThreadService.Add(chatThread);
            if (!result.success)
                return BadRequest(new { message = result.message });

            // Add creator as participant
            var creatorParticipant = new ThreadParticipant
            {
                ThreadId = chatThread.ThreadId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };
            
            _threadParticipantService.Add(creatorParticipant);

            // Add members
            if (request.Members != null && request.Members.Any())
            {
                foreach (var memberId in request.Members)
                {
                    // Verify user exists
                    var user = _userService.GetById(memberId);
                    if (user == null) continue;

                    var participant = new ThreadParticipant
                    {
                        ThreadId = chatThread.ThreadId,
                        UserId = memberId,
                        JoinedAt = DateTime.UtcNow
                    };
                    
                    _threadParticipantService.Add(participant);
                }
            }

            return Ok(new { 
                groupId = chatThread.ThreadId,
                groupName = chatThread.Title
            });
        }

        [HttpGet]
        public IActionResult GetUserGroups()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Get all threads where user is a participant and that are not private (groups)
            var participations = _threadParticipantService.GetByUserId(userId);
            var groupThreadIds = participations.Select(p => p.ThreadId).ToList();
            
            var groupThreads = _chatThreadService.GetAll()
                .Where(t => groupThreadIds.Contains(t.ThreadId) && !t.IsPrivate)
                .ToList();

            var result = groupThreads.Select(t => new {
                groupId = t.ThreadId,
                groupName = t.Title,
                createdAt = t.CreatedAt,
                lastActivity = t.LastMessageAt
            });

            return Ok(result);
        }

        [HttpGet("{groupId}")]
        public IActionResult GetGroupDetails(int groupId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Verify thread exists
            var thread = _chatThreadService.GetById(groupId);
            if (thread == null)
                return NotFound(new { message = "Group not found" });
            
            // Verify user is a participant
            var participants = _threadParticipantService.GetByThreadId(groupId);
            var userParticipant = participants.FirstOrDefault(p => p.UserId == userId);
            if (userParticipant == null)
                return StatusCode(403, new { message = "You are not a member of this group" });
            
            // Get all participants
            var memberIds = participants.Select(p => p.UserId).ToList();
            var members = _userService.GetAll().Where(u => memberIds.Contains(u.UserId)).ToList();
            
            var memberDetails = members.Select(m => new {
                userId = m.UserId,
                username = m.Username,
                isFirstMember = participants.OrderBy(p => p.JoinedAt).FirstOrDefault()?.UserId == m.UserId,
                joinedAt = participants.FirstOrDefault(p => p.UserId == m.UserId)?.JoinedAt
            });

            return Ok(new {
                groupId = thread.ThreadId,
                groupName = thread.Title,
                createdAt = thread.CreatedAt,
                lastActivity = thread.LastMessageAt,
                isFirstMember = participants.OrderBy(p => p.JoinedAt).FirstOrDefault()?.UserId == userId,
                members = memberDetails
            });
        }

        [HttpPut("{groupId}")]
        public IActionResult UpdateGroup(int groupId, [FromBody] UpdateGroupRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Verify thread exists
            var thread = _chatThreadService.GetById(groupId);
            if (thread == null)
                return NotFound(new { message = "Group not found" });
            
            // Verify user is a participant (ideally the creator, but we don't have that field)
            var participants = _threadParticipantService.GetByThreadId(groupId);
            var userParticipant = participants.FirstOrDefault(p => p.UserId == userId);
            if (userParticipant == null)
                return StatusCode(403, new { message = "You are not a member of this group" });
            
            // Check if user is the first participant (as a proxy for creator)
            var firstParticipant = participants.OrderBy(p => p.JoinedAt).FirstOrDefault();
            if (firstParticipant == null || firstParticipant.UserId != userId)
                return BadRequest(new { message = "Only the group creator can update the group" });
            
            // Update group details
            if (!string.IsNullOrEmpty(request.GroupName))
                thread.Title = request.GroupName;
            
            var result = _chatThreadService.Update(thread);
            if (!result.success)
                return BadRequest(new { message = result.message });

            return Ok(new { message = "Group updated successfully" });
        }

        [HttpDelete("{groupId}")]
        public IActionResult DeleteGroup(int groupId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Verify thread exists
            var thread = _chatThreadService.GetById(groupId);
            if (thread == null)
                return NotFound(new { message = "Group not found" });
            
            // Verify user is a participant (ideally the creator, but we don't have that field)
            var participants = _threadParticipantService.GetByThreadId(groupId);
            var userParticipant = participants.FirstOrDefault(p => p.UserId == userId);
            if (userParticipant == null)
                return StatusCode(403, new { message = "You are not a member of this group" });
            
            // Check if user is the first participant (as a proxy for creator)
            var firstParticipant = participants.OrderBy(p => p.JoinedAt).FirstOrDefault();
            if (firstParticipant == null || firstParticipant.UserId != userId)
                return BadRequest(new { message = "Only the group creator can delete the group" });
            
            // Delete the group (this will cascade delete participants)
            var result = _chatThreadService.Delete(groupId);
            if (!result)
                return BadRequest(new { message = "Failed to delete group" });

            return Ok(new { message = "Group deleted successfully" });
        }

        [HttpPost("{groupId}/members")]
        public IActionResult AddGroupMembers(int groupId, [FromBody] AddMembersRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Verify thread exists
            var thread = _chatThreadService.GetById(groupId);
            if (thread == null)
                return NotFound(new { message = "Group not found" });
            
            // Verify user is a participant (ideally the creator, but we don't have that field)
            var participants = _threadParticipantService.GetByThreadId(groupId);
            var userParticipant = participants.FirstOrDefault(p => p.UserId == userId);
            if (userParticipant == null)
                return StatusCode(403, new { message = "You are not a member of this group" });
            
            // Check if user is the first participant (as a proxy for creator)
            var firstParticipant = participants.OrderBy(p => p.JoinedAt).FirstOrDefault();
            if (firstParticipant == null || firstParticipant.UserId != userId)
                return BadRequest(new { message = "Only the group creator can add members" });

            // Add new members
            foreach (var memberId in request.UserIds)
            {
                // Skip if already a member
                if (participants.Any(p => p.UserId == memberId))
                    continue;
                
                // Verify user exists
                var user = _userService.GetById(memberId);
                if (user == null) continue;

                var newParticipant = new ThreadParticipant
                {
                    ThreadId = groupId,
                    UserId = memberId,
                    JoinedAt = DateTime.UtcNow
                };
                
                _threadParticipantService.Add(newParticipant);
            }

            return Ok(new { message = "Members added successfully" });
        }

        [HttpDelete("{groupId}/members/{userId:int}")]
        public IActionResult RemoveGroupMember(int groupId, int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Verify thread exists
            var thread = _chatThreadService.GetById(groupId);
            if (thread == null)
                return NotFound(new { message = "Group not found" });
            
            // Get all participants
            var participants = _threadParticipantService.GetByThreadId(groupId);
            var currentUserParticipant = participants.FirstOrDefault(p => p.UserId == currentUserId);
            
            // Verify current user is a member
            if (currentUserParticipant == null)
                return StatusCode(403, new { message = "You are not a member of this group" });
            
            // Check if removing self or if user is the first participant (creator)
            var firstParticipant = participants.OrderBy(p => p.JoinedAt).FirstOrDefault();
            var isCreator = firstParticipant != null && firstParticipant.UserId == currentUserId;
            
            // Only allow self-removal or creator removing others
            if (currentUserId != userId && !isCreator)
                return BadRequest(new { message = "Only the group creator can remove other members" });
            
            // Find the participant to remove
            var participantToRemove = participants.FirstOrDefault(p => p.UserId == userId);
            if (participantToRemove == null)
                return NotFound(new { message = "User is not a member of this group" });
            
            // Use RemoveUserFromThread method which is available in the service
            var result = _threadParticipantService.RemoveUserFromThread(userId, groupId);
            if (!result)
                return BadRequest(new { message = "Failed to remove member from group" });

            return Ok(new { message = "Member removed successfully" });
        }
    }

    public class CreateGroupRequest
    {
        public string GroupName { get; set; } = string.Empty;
        public List<int> Members { get; set; } = new List<int>();
    }

    public class UpdateGroupRequest
    {
        public string GroupName { get; set; } = string.Empty;
    }

    public class AddMembersRequest
    {
        public List<int> UserIds { get; set; } = new List<int>();
    }
} 