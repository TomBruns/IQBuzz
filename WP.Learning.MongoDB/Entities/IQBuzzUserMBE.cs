using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace WP.Learning.MongoDB.Entities
{
    public class IQBuzzUserMBE
    {
        public const string COLLECTION_NAME = @"gfos2.iq_buzz_users";

        [BsonId]
        public ObjectId ID { get; set; }

        public int user_id { get; set; }

        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone_no { get; set; }
        public string email_address { get; set; }
        public string local_time_zone { get; set; }
        public bool has_accepted_welcome_agreement { get; set; }

        public List<int> merchant_ids { get; set; }

        // generic code to support "upcasting" to derived classes
        // this is expensive (ie uses reflection) so use sparingly
        public T As<T>()
        {
            var type = typeof(T);
            var instance = Activator.CreateInstance(type);

            if (type.BaseType != null)
            {
                var properties = type.BaseType.GetProperties();
                foreach (var property in properties)
                    if (property.CanWrite)
                        property.SetValue(instance, property.GetValue(this, null), null);
            }

            return (T)instance;
        }
    }
}
