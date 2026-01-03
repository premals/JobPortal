using IdendityService.Models;
using MongoDB.Driver;

namespace IdendityService.Helper
{
    public static class SetupMongoIndexes
    {
        public static async Task ConfigureMongoIndexes(IMongoDatabase db)
        {
            var users = db.GetCollection<ApplicationUser>("Users");

            var emailIndex = Builders<ApplicationUser>.IndexKeys.Ascending(u => u.Email);

            await users.Indexes.CreateManyAsync(new[]
            {
        new CreateIndexModel<ApplicationUser>(emailIndex, new CreateIndexOptions { Unique = true }),
    });
        }

    }
}
