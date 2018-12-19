using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WP.Learning.MongoDB.Entities
{
    public class UserActivityMBE
    {
        public const string COLLECTION_NAME = @"gfos2.user_activity";

        [BsonId]
        public ObjectId ID { get; set; }

        public string phone_no { get; set; }
        public DateTime activity_dt { get; set; }
        public string action { get; set; }
        public string comments { get; set; }

    }
}
