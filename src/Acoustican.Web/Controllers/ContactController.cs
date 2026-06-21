using Acoustican.Models;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/contact")]
[Authorize(Roles = "Admin,ContentManager,Viewer")]
public class ContactController(IContactService contactService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateContactMessage([FromBody] ContactMessage message)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        message.CreatedAt = DateTime.UtcNow;
        var createdMessage = await contactService.CreateContactMessageAsync(message);
        return CreatedAtAction(nameof(GetContactMessageById), new { id = createdMessage.Id }, createdMessage);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllContactMessages()
    {
        var messages = await contactService.GetAllContactMessagesAsync();
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetContactMessageById(int id)
    {
        var message = await contactService.GetContactMessageByIdAsync(id);
        if (message == null)
            return NotFound();
        return Ok(message);
    }

    [HttpPut("{id}/mark-read")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var message = await contactService.MarkAsReadAsync(id);
        if (message == null)
            return NotFound();
        return Ok(message);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> DeleteContactMessage(int id)
    {
        var success = await contactService.DeleteContactMessageAsync(id);
        if (!success)
            return NotFound();
        return Ok("Message deleted successfully");
    }
}
