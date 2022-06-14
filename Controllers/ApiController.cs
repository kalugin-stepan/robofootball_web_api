using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
        if (!PasswordIsValid(password)) return BadRequest();
        var id = await db.AddUser(new MqttUser() {username = username, password = password});
        if (id == 0) return BadRequest();
        await db.AddAcl(new MqttAcl() {user_id = id, topic="#", access = Access.subscribe});
        await db.AddAcl(new MqttAcl() {user_id = id, topic=id.ToString(), access = Access.publish});
        await db.AddAcl(new MqttAcl() {user_id = id, topic = $"{id}/#", access = Access.publish});
        return Ok(id.ToString());
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password) {
        var user = await db.GetUserByUsername(username);

        if (user == null || user.password != db.GetMD5Hash(password)) return BadRequest();

        return Ok(await GenerateToken(user));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromForm] uint id, [FromForm] string token) {
        var user = await db.GetUserById(id);
        if (user == null || user.token != token) return BadRequest();
        user.token = "";
        await db.UpdateUser(user);
        return Ok();
    }

    [HttpPost("is_token_valid")]
    public async Task<IActionResult> IsTokenValid([FromForm] uint id, [FromForm] string token) {
        var rez = await db.IsTokenValid(new Token(){id = id, token = token});
        if (rez) return Ok();
        return BadRequest();
    }

    [HttpPost("change_password")]
    public async Task<IActionResult> ChangePassword([FromForm] string username,
    [FromForm] string old_password, [FromForm] string new_password) {
        var user = await db.GetUserByUsername(username);
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

    private bool PasswordIsValid(string password) {
        try {
            int.Parse(password);
            return false;
        } catch {
            return password.Length >= 8;
        }
    }
    private string GenerateTokenString(int len) {
        var rand = new Random();
        char[] token = new char[len];
        for (int i = 0; i < len; i++) {
            token[i] = (char)rand.Next(48, 122);
        }
        return new string(token);
    }

    public async Task<Token> GenerateToken(MqttUser user) {
        var token_string = GenerateTokenString(36);
        var token = new Token() {
            id = user.id,
            token = token_string
        };
        user.token = token_string;
        await db.UpdateUser(user);
        return token;
    }
}