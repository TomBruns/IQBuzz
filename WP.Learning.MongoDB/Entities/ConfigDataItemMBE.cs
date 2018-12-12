using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WP.Learning.MongoDB.Entities
{
    /// <summary>
    /// This class represents a row (document) in the config_data_items collection
    /// </summary>
    public class ConfigDataItemMBE
    {
        public const string COLLECTION_NAME = @"gfos2.config_data_items";

        [BsonId]
        public ObjectId ID { get; set; }

        public string name { get; set; }
        public string value { get; set; }
    }
}
