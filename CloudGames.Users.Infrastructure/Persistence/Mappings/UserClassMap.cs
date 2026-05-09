using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence.Mappings
{
    public static class UserClassMap
    {
        private static bool _registered;
        private static readonly object _lock = new();

        public static void Register()
        {
            if (_registered) return;
            lock (_lock)
            {
                if (_registered) return;

                if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
                {
                    BsonClassMap.RegisterClassMap<User>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapIdProperty(u => u.Id);
                        cm.MapProperty(u => u.Role).SetSerializer(new EnumSerializer<UserRole>(MongoDB.Bson.BsonType.String));
                        cm.SetIgnoreExtraElements(true);
                    });
                }

                _registered = true;
            }
        }
    }
}
