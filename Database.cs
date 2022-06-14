using Microsoft.EntityFrameworkCore;
using Models;
using System.Security.Cryptography;
using System.Text;

public class Database: DbContext {
    private readonly MD5 md5 = MD5.Create();
    public Database(DbContextOptions<Database> options) : base(options) {}

    public DbSet<MqttUser> users { get; set; }
    public DbSet<MqttAcl> acls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<MqttUser>().ToTable("mqtt_user");
        modelBuilder.Entity<MqttAcl>().ToTable("mqtt_acl");
    }

    public async Task<List<MqttUser>> GetAllUsers() {
        return await users.ToListAsync();
    }

    public async Task<MqttUser?> GetUserById(uint id) {
        return await users.Where(user => user.id == id).FirstOrDefaultAsync();
    }

    public async Task<MqttUser?> GetUserByUsername(string username) {
        return await users.Where(user => user.username == username).FirstOrDefaultAsync();
    }

    public async Task<MqttUser?> GetUserByToken(string token) {
        return await users.Where(user => user.token == token).FirstOrDefaultAsync();
    }

    public async Task<uint> AddUser(MqttUser user) {
        if (await GetUserByUsername(user.username) != null) return 0;
        user.password = GetMD5Hash(user.password);
        var rez = await users.AddAsync(user);
        await SaveChangesAsync();
        return rez.Entity.id;
    }

    public async Task DeleteUser(uint id) {
        var user = users.Where(user => user.id == id).FirstOrDefault();
        if (user == null) return;
        users.Remove(user);
        await DeleteAclsByUserId(id);
        await SaveChangesAsync();
    }

    public async Task DeleteUser(string username) {
        var user = users.Where(user => user.username == username).FirstOrDefault();
        if (user == null) return;
        users.Remove(user);
        await DeleteAclsByUsername(username);
        await SaveChangesAsync();
    }

    public async Task UpdateUser(MqttUser user) {
        users.Update(user);
        await SaveChangesAsync();
    }

    public async Task<List<MqttAcl>> GetAllAcls() {
        return await acls.ToListAsync();
    }

    public async Task<MqttAcl?> GetAclById(uint id) {
        return await acls.Where(acl => acl.id == id).FirstOrDefaultAsync();
    }

    public async Task<ICollection<MqttAcl>> GetAclsByUserId(uint user_id) {
        return await acls.Where(acl => acl.user_id == user_id).ToListAsync();
    }

    public async Task<ICollection<MqttAcl>> GetAclsByUsername(string username) {
        return await acls.Where(acl => acl.username == username).ToListAsync();
    }

    public async Task<ICollection<MqttAcl>> GetAclsByTopic(string topic) {
        return await acls.Where(acl => acl.topic == topic).ToListAsync();
    }

    public async Task<bool> AddAcl(MqttAcl acl) {
        await acls.AddAsync(acl);
        await SaveChangesAsync();
        return true;
    }

    public async Task DeleteAclById(uint id) {
        var acl = await acls.Where(acl => acl.id == id).FirstOrDefaultAsync();
        if (acl == null) return;
        acls.Remove(acl);
        await SaveChangesAsync();
    }

    public async Task DeleteAclsByUserId(uint user_id) {
        await acls.Where(acl => acl.user_id == user_id).ForEachAsync(acl => acls.Remove(acl));
        await SaveChangesAsync();
    }

    public async Task DeleteAclsByUsername(string username) {
        await acls.Where(acl => acl.username == username).ForEachAsync(acl => acls.Remove(acl));
        await SaveChangesAsync();
    }

    public async Task DeleteAclsByTopic(string topic) {
        await acls.Where(acl => acl.topic == topic).ForEachAsync(acl => acls.Remove(acl));
        await SaveChangesAsync();
    }

    public async Task UpdateAcl(MqttAcl acl) {
        acls.Update(acl);
        await SaveChangesAsync();
    }

    public string GetMD5Hash(string data) {
        return Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }
    public async Task<bool> IsTokenValid(Token token) {
        if (token.token == "") return false;
        var user = await GetUserById(token.id);
        if (user == null) return false;
        return token.token == user.token;
    }
}