using LiteDB;

namespace Prototype
{
    public abstract class DatabaseObject
    {
        [BsonId(autoId: false)]
        public int uniqueId { get; set; }

        public void AddToSaveQueue<T>() where T : DatabaseObject
        {
            Database.AddToSaveQueue(this as T);
        }
    }
}
