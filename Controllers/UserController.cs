using Microsoft.AspNetCore.Mvc;
using Models;
using Helpers;

namespace Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase {
    private readonly Database db;
    public UserController(Database db) {
        this.db = db;
    }

    [HttpPost("")]
    public async Task<IActionResult> Users() {
        return Ok(await db.GetAllUsers());
    }

    [HttpPost("select")]
    public new async Task<IActionResult> User([FromForm] uint? id, [FromForm] string? username) {
        if (id != null) {
            var user = await db.GetUser(id.Value);
            return Ok(user);
        }
        if (username != null) {
            var user = await db.GetUser(username);
            return Ok(user);
        }
        return BadRequest();
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromForm]MqttUser user) {
        if (!ModelState.IsValid) return BadRequest();
        var id = await db.AddUser(user);
        if (id == 0) return BadRequest();
        return Ok(id);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromForm] uint? id, [FromForm] string? username) {
        if (id != null) {
            await db.DeleteUser(id.Value);
            return Ok();
        }
        if (username != null) {
            await db.DeleteUser(username);
            return Ok();
        }
        return BadRequest();
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(MqttUser user) {
        if (!ModelState.IsValid) return BadRequest();
        await db.UpdateUser(user);
        return Ok();
    }

    
}