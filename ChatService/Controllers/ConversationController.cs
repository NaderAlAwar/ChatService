using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Logging;
using ChatService.Notifications;
using ChatService.Services;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Metrics;

namespace ChatService.Controllers
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ConversationController : Controller
    {
        private readonly ILogger<ConversationController> logger;
        private readonly IConversationService conversationService;

        public ConversationController(ILogger<ConversationController> logger, IConversationService conversationService)
        {
            this.logger = logger;
            this.conversationService = conversationService;
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> ListMessages(string conversationId, [FromQuery] string startCt, [FromQuery] string endCt, [FromQuery] int limit)
        {
            try
            {
                var listMessagesDto = await conversationService.ListMessages(conversationId, startCt, endCt, limit);
                return Ok(listMessagesDto);
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e, 
                    "Could not reach storage to list messages, conversationId {conversationId}", conversationId);
                return StatusCode(503, $"Could not reach storage to list messages, conversationId {conversationId}");
            }
            catch (Exception e)
            {
                logger.LogError(Events.InternalError, e, 
                    "Failed to list messages of conversation {conversationId}", conversationId);
                return StatusCode(500, $"Failed to list messages of conversation {conversationId}");
            }
        }

        [HttpPost("{id}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> PostMessage(string id, [FromBody] SendMessageDto messageDto)
        {
            try
            {
                var message = await conversationService.PostMessage(id, messageDto);
                return Ok(message);
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e, 
                    "Could not reach storage to add message, conversationId {conversationId}", id);
                return StatusCode(503, $"Could not reach storage to add message, conversationId {id}");
            }
            catch (Exception e)
            {
                logger.LogError(Events.InternalError, e, 
                    "Failed to add message to conversation, conversationId: {conversationId}", id);
                return StatusCode(500, $"Failed to add message to conversation, conversationId: {id}");
            }
        }

        [HttpPost("{id}")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> PostMessage(string id, [FromBody] SendMessageDtoV2 messageDto)
        {
            try
            {
                var message = await conversationService.PostMessage(id, messageDto);
                return Ok(message);
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e,
                    "Could not reach storage to add message, conversationId {conversationId}", id);
                return StatusCode(503, $"Could not reach storage to add message, conversationId {id}");
            }
            catch (Exception e)
            {
                logger.LogError(Events.InternalError, e,
                    "Failed to add message to conversation, conversationId: {conversationId}", id);
                return StatusCode(500, $"Failed to add message to conversation, conversationId: {id}");
            }
        }
    }
}