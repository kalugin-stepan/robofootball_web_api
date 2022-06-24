using Microsoft.AspNetCore.Mvc;
using Models;

[Route("[controller]")]
public class MqttController: ControllerBase {
    private readonly Database db;
    public MqttController(Database db) {
        this.db = db;
    }

    [HttpPost("is_logged_in")]
    public async Task<IActionResult> IsLoggedIn([FromForm] string username, [FromForm] string password) {
        if (username[0] == '~') {
            username = username.Substring(1);
            var user = await db.GetUserByUsername(username);
            if (user == null || user.password != db.GetMD5Hash(password)) return BadRequest();
            return Ok();
        }
        try {
            var id = uint.Parse(username);
            var user = await db.GetUserById(id);
            if (user == null || user.token != password) return BadRequest();
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPost("has_access")]
    public async Task<IActionResult> HasAccess([FromForm] string username, [FromForm] string topic, [FromForm] Access access) {
        if (username[0] == '~') {
            username = username.Substring(1);
            var user = await db.GetUserByUsername(username);
            if (user == null) return BadRequest();
            if (access == Access.publish && topic == user.id.ToString()) {
                return Ok();
            }
            if (access == Access.publish) {
                return BadRequest();
            }
            return Ok();
        }
        try {
            var id = uint.Parse(username);
            if (access == Access.subscribe) {
                return Ok();
            }
            var acls = await db.GetAclsByUserId(id);
            foreach (var acl in acls) {
                if ((acl.access == access || acl.access == Access.pubsub) && TopicMatch(acl.topic, topic)) {
                    return Ok();
                }
            }
            return BadRequest();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("is_superuser")]
    public async Task<IActionResult> IsSuperUser([FromForm] string username) {
        var user = await db.GetUserByUsername(username);
        if (user == null || !user.is_superuser) return BadRequest();
        return Ok();
    }

    public bool TopicMatch(string allowedTopic, string topic) {
        if (string.IsNullOrWhiteSpace(allowedTopic) || string.IsNullOrWhiteSpace(topic)) {
            return false;
        }

        if (allowedTopic == topic) {
            return true;
        }

        var topicLength = topic.Length;
        var allowedTopicLength = allowedTopic.Length;
        var position = 0;
        var allowedTopicIndex = 0;
        var topicIndex = 0;

        if ((allowedTopic[allowedTopicIndex] == '$' && topic[topicIndex] != '$') || (topic[topicIndex] == '$' && allowedTopic[allowedTopicIndex] != '$')) {
            return true;
        }

        while (allowedTopicIndex < allowedTopicLength) {
            if (topic[topicIndex] == '+' || topic[topicIndex] == '#') {
                return false;
            }

            if (allowedTopic[allowedTopicIndex] != topic[topicIndex] || topicIndex >= topicLength) {
                if (allowedTopic[allowedTopicIndex] == '+') {
                    if (position > 0 && allowedTopic[allowedTopicIndex - 1] != '/') {
                        return false;
                    }

                    if (allowedTopicIndex + 1 < allowedTopicLength && allowedTopic[allowedTopicIndex + 1] != '/') {
                        return false;
                    }

                    position++;
                    allowedTopicIndex++;
                    while (topicIndex < topicLength && topic[topicIndex] != '/') {
                        topicIndex++;
                    }

                    if (topicIndex >= topicLength && allowedTopicIndex >= allowedTopicLength) {
                        return true;
                    }
                }
                else if (allowedTopic[allowedTopicIndex] == '#') {
                    if (position > 0 && allowedTopic[allowedTopicIndex - 1] != '/') {
                        return false;
                    }

                    if (allowedTopicIndex + 1 < allowedTopicLength) {
                        return false;
                    }
                    else {
                        return true;
                    }
                }
                else {
                    if (topicIndex >= topicLength && position > 0 && allowedTopic[allowedTopicIndex - 1] == '+' && allowedTopic[allowedTopicIndex] == '/' && allowedTopic[allowedTopicIndex + 1] == '#') {
                        return true;
                    }

                    while (allowedTopicIndex < allowedTopicLength) {
                        if (allowedTopic[allowedTopicIndex] == '#' && allowedTopicIndex + 1 < allowedTopicLength) {
                            return false;
                        }

                        position++;
                        allowedTopicIndex++;
                    }

                    return false;
                }
            }
            else {
                if (topicIndex + 1 >= topicLength) {
                    if (allowedTopic[allowedTopicIndex + 1] == '/' && allowedTopic[allowedTopicIndex + 2] == '#' && allowedTopicIndex + 3 >= allowedTopicLength) {
                        return true;
                    }
                }

                position++;
                allowedTopicIndex++;
                topicIndex++;

                if (allowedTopicIndex >= allowedTopicLength && topicIndex >= topicLength) {
                    return true;
                }
                else if (topicIndex >= topicLength && allowedTopic[allowedTopicIndex] == '+' && allowedTopicIndex + 1 >= allowedTopicLength) {
                    if (position > 0 && allowedTopic[allowedTopicIndex - 1] != '/') {
                        return false;
                    }

                    position++;
                    allowedTopicIndex++;

                    return true;
                }
            }
        }

        if (topicIndex < topicLength || allowedTopicIndex < allowedTopicLength)
        {
            return false;
        }

        return true;
    }
}