using Microsoft.AspNetCore.Mvc;
using Models;

namespace Controllers;

[Route("[controller]")]
public class ApiController : ControllerBase {
    private readonly Database db;
    public ApiController(Database db) {
        this.db = db;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] string username, [FromForm] string password) {
        var id = await db.AddUser(new MqttUser() {username = username, password = password});
        if (id == 0) return BadRequest();
        await db.AddAcl(new MqttAcl() {username = username, topic="#", access = Access.subscribe});
        await db.AddAcl(new MqttAcl() {username = username, topic = $"{id}/#", access = Access.publish});
        return Ok(id.ToString());
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password) {
        var user = await db.GetUser(username);

        if (user == null || user.password != db.GetMD5Hash(password)) return BadRequest();

        return Ok(user.id.ToString());
    }

    [HttpPost("change_password")]
    public async Task<IActionResult> ChangePassword([FromForm] string username,
    [FromForm] string old_password, [FromForm] string new_password) {
        var user = await db.GetUser(username);
        if (user == null) return BadRequest();
        if (user.password != db.GetMD5Hash(old_password)) return BadRequest();
        user.password = db.GetMD5Hash(new_password);
        await db.UpdateUser(user);
        return Ok();
    }

    [HttpPost("register_viewer")]
    public async Task<IActionResult> RegisterViewer([FromForm] string username, [FromForm] string password) {
        var id = await db.AddUser(new MqttUser() {username = username, password = password});
        if (id == 0) return BadRequest("User already exists");
        return Ok(id.ToString());
    }
}