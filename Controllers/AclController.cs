using Microsoft.AspNetCore.Mvc;
using Models;
using Helpers;

namespace Controllers;

[Route("[controller]")]
[Authorize]
[ApiController]
public class AclController : ControllerBase {
    private readonly Database db;
    public AclController(Database db) {
        this.db = db;
    }

    [HttpPost("")]
    public async Task<IActionResult> Acls() {
        return Ok(await db.GetAllAcls());
    }

    [HttpPost("select")]
    public async Task<IActionResult> Acl([FromForm] uint? id, [FromForm] string? username,
    [FromForm] string? topic, [FromForm] uint? user_id) {
        if (id != null) {
            return Ok(await db.GetAclById(id.Value));
        }
        if (user_id != null) {
            return Ok(await db.GetAclsByUserId(user_id.Value));
        }
        if (username != null) {
            return Ok(await db.GetAclsByUsername(username));
        }
        if (topic != null) {
            return Ok(await db.GetAclsByTopic(topic));
        }
        return BadRequest();
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromForm] MqttAcl acl) {
        if (!MqttAclIsValid(acl)) return BadRequest();
        var rez = await db.AddAcl(acl);
        if (!rez) return BadRequest();
        return Ok();
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromForm] uint? id, [FromForm] string? username,
    [FromForm] string? topic, [FromForm] uint? user_id) {
        if (id != null) {
            await db.DeleteAclById(id.Value);
            return Ok();
        }
        if (user_id != null) {
            await db.DeleteAclsByUserId(user_id.Value);
            return Ok();
        }
        if (username != null) {
            await db.DeleteAclsByUsername(username);
            return Ok();
        }
        if (topic != null) {
            await db.DeleteAclsByTopic(topic);
        }
        return BadRequest();
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromForm] MqttAcl acl) {
        if (!ModelState.IsValid) return BadRequest();
        await db.UpdateAcl(acl);
        return Ok();
    }

    private bool MqttAclIsValid(MqttAcl acl) {

        return acl.user_id != 0 && acl.topic != "";
    }
}