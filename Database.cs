using Models;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

public class Database {
    private readonly MySqlConnection connection;
    private readonly MD5 md5 = MD5.Create();
    public Database(string connectionString) {
        connection = new(connectionString);
        connection.Open();
        InitialCreate();
    }

    ~Database() {
        connection.Close();
    }

    private void InitialCreate() {
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"create table if not exists mqtt_user (
            id int(11) unsigned NOT NULL AUTO_INCREMENT PRIMARY KEY UNIQUE,
            username varchar(100) NOT NULL UNIQUE,
            password varchar(100) NOT NULL,
            is_superuser tinyint(1) DEFAULT 0 NOT NULL
        );";
        cmd.ExecuteNonQuery();
        cmd.CommandText = @"CREATE TABLE if not exists mqtt_acl (
            id int(11) unsigned NOT NULL AUTO_INCREMENT PRIMARY KEY,
            user_id int(11) unsigned NOT NULL,
            username varchar(100) NOT NULL,
            access tinyint(1) NOT NULL,
            topic varchar(100) NOT NULL DEFAULT '',
            index (username),
            FOREIGN KEY (user_id) REFERENCES mqtt_user (id)
        );";
        cmd.ExecuteNonQuery();
    }

    public async Task<List<MqttUser>> GetAllUsers() {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "select * from mqtt_user;";
        var reader = await cmd.ExecuteReaderAsync();
        var users = new List<MqttUser>();
        while (await reader.ReadAsync()) {
            users.Add(new MqttUser() {
                username = reader.GetString(1),
                password = reader.GetString(2),
                is_superuser = reader.GetInt16(3) == 1
            });
        }
        return users;
    }

    public async Task<MqttUser?> GetUser(uint id) {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"select * from mqtt_user where id = {id} limit 1;";
        var reader = await cmd.ExecuteReaderAsync();
        
        if (!await reader.ReadAsync()) {
            await reader.CloseAsync();
            return null;
        }
        var user = new MqttUser() {
            username = reader.GetString(1),
            password = reader.GetString(2),
            is_superuser = reader.GetInt16(3) == 1
        };

        await reader.CloseAsync();
        return user;
    }

    public async Task<MqttUser?> GetUser(string username) {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"select * from mqtt_user where username = '{username}' limit 1;";
        var reader = await cmd.ExecuteReaderAsync();
        
        if (!await reader.ReadAsync()) {
            await reader.CloseAsync();
            return null;
        }
        var user = new MqttUser() {
            id = await reader.GetFieldValueAsync<uint>(0),
            username = reader.GetString(1),
            password = reader.GetString(2),
            is_superuser = reader.GetInt16(3) == 1
        };

        await reader.CloseAsync();
        return user;
    }

    public async Task<uint> AddUser(MqttUser user) {

        if (await GetUser(user.username) != null) return 0;

        var cmd = connection.CreateCommand();

        var is_superuser = user.is_superuser ? 1 : 0;
        var password_hash = GetMD5Hash(user.password);

        cmd.CommandText = @$"insert into mqtt_user (username, password, is_superuser)
        values ('{user.username}', '{password_hash}', {is_superuser});";
        
        await cmd.ExecuteNonQueryAsync();

        cmd.CommandText = "select id from mqtt_user order by id desc limit 1;";

        var id_str = await cmd.ExecuteScalarAsync();

        Console.WriteLine(id_str);

        var id = Convert.ToUInt32(id_str);

        return id;
    }

    public async Task DeleteUser(uint id) {
        await DeleteAclByUserId(id);
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"delete from mqtt_user where id = {id};";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteUser(string username) {
        await DeleteAclByUsername(username);
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"delete from mqtt_user where username = '{username}';";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateUser(MqttUser user) {
        var cmd = connection.CreateCommand();
        var is_superuser = user.is_superuser ? 1 : 0;
        var password_hash = GetMD5Hash(user.password);
        cmd.CommandText = $@"upadte mqtt_user
        set username = '{user.username}', password = '{password_hash}', is_superuser = {is_superuser} where id = '{user.id}';";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<MqttAcl>> GetAllAcls() {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "select * from mqtt_acl;";
        var reader = await cmd.ExecuteReaderAsync();
        var acls = new List<MqttAcl>();
        while (await reader.ReadAsync()) {
            acls.Add(new MqttAcl() {
                username = reader.GetString(2),
                access = (Access)reader.GetInt16(3),
                topic = reader.GetString(4)
            });
        }
        return acls;
    }

    public async Task<MqttAcl?> GetAclById(uint id) {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"select * from mqtt_acl where id = {id}";
        var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) {
            await reader.CloseAsync();
            return null;
        }
        var user = new MqttAcl() {
            id = await reader.GetFieldValueAsync<uint>(0),
            username = reader.GetString(1),
            access = (Access)reader.GetInt16(2),
            topic = reader.GetString(3)
        };
        return user;
    }

    public async Task<MqttAcl?> GetAclByUserId(uint user_id) {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"select * from mqtt_acl where id = {user_id}";
        var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) {
            await reader.CloseAsync();
            return null;
        }
        var user = new MqttAcl() {
            id = await reader.GetFieldValueAsync<uint>(0),
            username = reader.GetString(1),
            access = (Access)reader.GetInt16(2),
            topic = reader.GetString(3)
        };
        return user;
    }

    public async Task<MqttAcl?> GetAclByUsername(string username) {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"select * from mqtt_acl where username = '{username}';";
        var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) {
            await reader.CloseAsync();
            return null;
        }
        var user = new MqttAcl() {
            username = reader.GetString(1),
            access = (Access)reader.GetInt16(2),
            topic = reader.GetString(3)
        };
        await reader.CloseAsync();
        return user;
    }

    public async Task<MqttAcl?> GetAclByTopic(string topic) {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"select * from mqtt_acl where topic = '{topic}';";
        var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) {
            await reader.CloseAsync();
            return null;
        }
        var user = new MqttAcl() {
            username = reader.GetString(1),
            access = (Access)reader.GetInt16(2),
            topic = reader.GetString(3)
        };
        return user;
    }

    public async Task<bool> AddAcl(MqttAcl acl) {
        var cmd = connection.CreateCommand();

        var access = (Int16)acl.access;

        var user = await GetUser(acl.user_id);

        if (user == null) {
            return false;
        }

        cmd.CommandText = $"insert into mqtt_acl (user_id, username, access, topic) values ('{acl.user_id}', '{user.username}', {access}, '{acl.topic}');";
    
        await cmd.ExecuteNonQueryAsync();

        return true;
    }

    public async Task DeleteAclById(uint id) {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"delete from mqtt_acl where id = {id};";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAclByUserId(uint id) {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"delete from mqtt_acl where user_id = {id};";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAclByUsername(string username) {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"delete from mqtt_acl where username = '{username}';";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAclByTopic(string topic) {
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"delete from mqtt_acl where topic = '{topic}';";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAcl(MqttAcl acl) {
        var cmd = connection.CreateCommand();
        var access = (Int16)acl.access;
        cmd.CommandText = $"update mqtt_acl set (username, access, topic) values ('{acl.username}', {access}, '{acl.topic}');";
        await cmd.ExecuteNonQueryAsync();
    }

    public string GetMD5Hash(string data) {
        return Encoding.UTF8.GetString(md5.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }
}