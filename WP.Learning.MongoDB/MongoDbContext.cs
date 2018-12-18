using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using MongoDB.Bson;
using MongoDB.Driver;

using WP.Learning.MongoDB.Entities;

namespace WP.Learning.MongoDB
{

    /// <summary>
    /// This is the main interface between the hackathon application and MongoDB
    /// </summary>
    // References
    // https://docs.mongodb.com/manual/tutorial/install-mongodb-on-windows/
    // http://mongodb.github.io/mongo-csharp-driver/2.4/getting_started/quick_tour/
    // https://docs.mongodb.com/compass/current/#compass-index
    // http://www.layerworks.com/blog/2014/11/11/mongodb-shell-csharp-driver-comparison-cheat-cheet
    // https://mongodb-documentation.readthedocs.io/en/latest/ecosystem/tutorial/use-linq-queries-with-csharp-driver.html
    // http://mongodb.github.io/mongo-csharp-driver/2.7/reference/driver/crud/writing/#insert
    public static class MongoDBContext
    {
        const bool IS_USE_LOCAL = false;

        const string LOCAL_SERVER_NAME = @"localhost";
        const int LOCAL_PORT_NUMBER = 27017;

        const string CLOUD_HOST_NAME = @"wpiqbuzzmongodb.documents.azure.com";
        const int CLOUD_PORT_NUMBER = 10255;
        const string CLOUD_USERNAME = @"wpiqbuzzmongodb";
        const string CLOUD_PASSWORD = @"D4zUXpJDDBhqQi8w76IDgaipo8Azqr4NcVLJOajyuJQPEDW42GAUuAQUcQUVyDvx7uz95EDO2To7huauoS3O9w==";

        const string DB_NAME = @"gfos2";

        internal static MongoClient GetMongoClient()
        {
            MongoClient client = null;

            if (IS_USE_LOCAL)
            {
                // connect to local Mongo Instance
                client = new MongoClient($"mongodb://{LOCAL_SERVER_NAME}:{LOCAL_PORT_NUMBER}");
            }
            else
            {
                MongoClientSettings settings = new MongoClientSettings()
                {
                    Server = new MongoServerAddress(CLOUD_HOST_NAME, CLOUD_PORT_NUMBER),
                    UseSsl = true,
                    SslSettings = new SslSettings()
                    {
                        EnabledSslProtocols = SslProtocols.Tls12
                    }
                };

                MongoIdentity identity = new MongoInternalIdentity(DB_NAME, CLOUD_USERNAME);
                MongoIdentityEvidence evidence = new PasswordEvidence(CLOUD_PASSWORD);

                settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

                // connect to Cloud Cosmos acting like Mongo Instance
                client = new MongoClient(settings);
            }

            return client;
        }

        public static void BootstrapMongoSchema()
        {
            var client = GetMongoClient();

            // will create the DB if it does not exist
            var db = client.GetDatabase(DB_NAME);

            #region ConfigDataMBE
            // will create the collection if it does not exist
            var configData = db.GetCollection<ConfigDataItemMBE>(ConfigDataItemMBE.COLLECTION_NAME);

            // create index on StdRequestTypeMBE
            //configData.Indexes.CreateOne(Builders<ConfigDataItemMBE>.IndexKeys.Ascending(_ => _.name), new CreateIndexOptions() { Unique = true });

            // 1=asc, -1=desc
            IndexKeysDefinition<ConfigDataItemMBE> cdeKeys = "{ name : 1 }";
            configData.Indexes.CreateOne(new CreateIndexModel<ConfigDataItemMBE>(cdeKeys, new CreateIndexOptions() { Unique = true }));
            #endregion

            #region MerchantMBE
            // will create the collection if it does not exist
            var merchants = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            // create index on StdRequestTypeMBE
            //merchants.Indexes.CreateOne(Builders<MerchantMBE>.IndexKeys.Ascending(_ => _.merchant_id), new CreateIndexOptions() { Unique = true });

            IndexKeysDefinition<MerchantMBE> mKeys = "{ merchant_id : 1 }";
            merchants.Indexes.CreateOne(new CreateIndexModel<MerchantMBE>(mKeys, new CreateIndexOptions() { Unique = true }));
            #endregion

            #region MerchantDailyActivityMBE
            // will create the collection if it does not exist
            var merchantDailyActivity = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            // create index on StdHierRequestTypeMBE
            //merchantDailyActivity.Indexes.CreateOne(Builders<MerchantDailyActivityMBE>.IndexKeys.Combine(
                        //    Builders<MerchantDailyActivityMBE>.IndexKeys.Ascending(f => f.merchant_id),
                        //    Builders<MerchantDailyActivityMBE>.IndexKeys.Ascending(f => f.xct_posting_date)
                        //),
                        //new CreateIndexOptions() { Unique = true });

            IndexKeysDefinition<MerchantDailyActivityMBE> mDAKeys = "{ merchant_id : 1,  xct_posting_date : 1}";
            merchantDailyActivity.Indexes.CreateOne(new CreateIndexModel<MerchantDailyActivityMBE>(mDAKeys, new CreateIndexOptions() { Unique = true }));

            #endregion
        }

        // ==== ConfigDataMBE ====================================
        public static List<ConfigDataItemMBE> GetAllConfigData()
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<ConfigDataItemMBE>(ConfigDataItemMBE.COLLECTION_NAME);

            // Finding all the documents in a collection is done with an empty filter, only expect to have 1
            var filter = new BsonDocument();
            var allConfigData = collection.Find(filter).ToList();

            return allConfigData;
        }

        public static string GetConfigItemValue(string configItemName)
        {
            var allConfigData = GetAllConfigData();

            return allConfigData.Where(d => d.name == configItemName).FirstOrDefault().value;
        }

        public static void InsertConfigData(string configItemName, string configItemData)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<ConfigDataItemMBE>(ConfigDataItemMBE.COLLECTION_NAME);

            collection.InsertOne(new ConfigDataItemMBE() { name = configItemName, value = configItemData });
        }

        public static DeleteResult DeleteAllConfigData()
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);

            var collection = db.GetCollection<ConfigDataItemMBE>(ConfigDataItemMBE.COLLECTION_NAME);

            var filter = new BsonDocument();
            var deleteResult = collection.DeleteMany(filter);

            return deleteResult;
        }

        // ==== MerchantMBE ====================================
        public static MerchantMBE FindMerchantById(int merchant_id)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            var filter = Builders<MerchantMBE>.Filter.Where(_ => _.merchant_id == merchant_id);

            var requestInstance = collection.Find(filter);

            if (requestInstance == null || requestInstance.CountDocuments() == 0)
            {
                return null;
            }
            else
            {
                return requestInstance.First();
            }
        }

        public static MerchantMBE FindMerchantByPrimaryContactPhoneNo(string phone_no)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            var filter = Builders<MerchantMBE>.Filter.Where(_ => _.primary_contact.phone_no == phone_no);

            var requestInstance = collection.Find(filter);

            if (requestInstance == null || requestInstance.CountDocuments() == 0)
            {
                return null;
            }
            else
            {
                return requestInstance.First();
            }
        }

        public static void InsertMerchant(MerchantMBE merchant)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            collection.InsertOne(merchant);
        }

        public static void UpdateMerchant(MerchantMBE merchant)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            var filter = new BsonDocument("_id", merchant.ID);
            collection.ReplaceOne(filter, merchant);
        }

        public static DeleteResult DeleteMerchant(int merchant_id)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);

            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            var deleteResult = collection.DeleteOne(_ => _.merchant_id == merchant_id);

            return deleteResult;
        }

        public static DeleteResult DeleteAllMerchants()
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);

            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            var deleteResult = collection.DeleteMany(_ => _.merchant_id != 0);

            return deleteResult;
        }

        // ==== MerchantDailyActivityMBE ========================
        public static MerchantDailyActivityMBE FindMerchantDailyActivity(int merchant_id, DateTime xct_posting_date)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var filter = Builders<MerchantDailyActivityMBE>.Filter.Where(_ => _.merchant_id == merchant_id && _.xct_posting_date == xct_posting_date);

            var queryResults = collection.Find(filter);

            if(queryResults != null && queryResults.CountDocuments() > 0)
            {
                return queryResults.First();
            }
            else
            {
                return null;
            }
        }

        public static void InsertMerchantDailyActivity(MerchantDailyActivityMBE merchantDailyActivity)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            collection.InsertOne(merchantDailyActivity);
        }

        public static void UpdateMerchantDailyActivity(MerchantDailyActivityMBE merchantDailyActivity)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var filter = new BsonDocument("_id", merchantDailyActivity.ID);
            collection.ReplaceOne(filter, merchantDailyActivity);
        }

        // this one is thread safe daily_transactions
        public static void UpsertMerchantDailyActivity(int merchant_id, DateTime xct_posting_date, List<TransactionMBE> transactions)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var merchantDailyActivity = FindMerchantDailyActivity(merchant_id, xct_posting_date);

            // start with the values passed in (so we keep the newest version)
            List<TransactionMBE> mergedTransactions = new List<TransactionMBE>(transactions);

            // init collection if reqd
            if (merchantDailyActivity.transactions == null)
            {
                merchantDailyActivity.transactions = new List<TransactionMBE>();
            }

            #region avoid overriding (merge) exisiting step data
            if (merchantDailyActivity.transactions == null)
            {
                merchantDailyActivity.transactions = new List<TransactionMBE>();
            }

            // there must be a better way to do this????
            foreach (var existingTransaction in merchantDailyActivity.transactions)
            {
                // see if we are updating the value
                var matchingTransaction = transactions.Where(xct => xct.xct_id == existingTransaction.xct_id).FirstOrDefault();

                // add the existing xct if it does not exist in the set passed in
                if (matchingTransaction == null)
                {
                    mergedTransactions.Add(existingTransaction);
                }
            }
            #endregion

            collection.FindOneAndUpdate<MerchantDailyActivityMBE>(
                        mda => mda.merchant_id == merchant_id && mda.xct_posting_date == xct_posting_date,
                        Builders<MerchantDailyActivityMBE>.Update.Set(mda => mda.transactions, mergedTransactions),
                        new FindOneAndUpdateOptions<MerchantDailyActivityMBE, MerchantDailyActivityMBE>() { IsUpsert = true }
                       );
        }

        public static void UpsertMerchantDailyActivity(int merchant_id, DateTime xct_posting_date, List<TerminalStatusMBE> terminalsStatus)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);
            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var merchantDailyActivity = FindMerchantDailyActivity(merchant_id, xct_posting_date);

            // start with the values passed in (so we keep the newest version)
            List<TerminalStatusMBE> mergedTerminalsStatus = new List<TerminalStatusMBE>(terminalsStatus);

            // init collection if reqd
            if(merchantDailyActivity.terminals_status == null)
            {
                merchantDailyActivity.terminals_status = new List<TerminalStatusMBE>();
            }

            #region avoid overriding (merge) exisiting step data
            // there must be a better way to do this????
            foreach (var existingTerminalStatus in merchantDailyActivity.terminals_status)
            {
                // see if we are updating the value
                var matchingTerminalStatus = terminalsStatus.Where(t => t.terminal_id == existingTerminalStatus.terminal_id).FirstOrDefault();

                // add the existing xct if it does not exist in the set passed in
                if (matchingTerminalStatus == null)
                {
                    mergedTerminalsStatus.Add(existingTerminalStatus);
                }
            }
            #endregion

            collection.FindOneAndUpdate<MerchantDailyActivityMBE>(
                        mda => mda.merchant_id == merchant_id && mda.xct_posting_date == xct_posting_date,
                        Builders<MerchantDailyActivityMBE>.Update.Set(mda => mda.terminals_status, mergedTerminalsStatus),
                        new FindOneAndUpdateOptions<MerchantDailyActivityMBE, MerchantDailyActivityMBE>() { IsUpsert = true }
                       );
        }

        public static DeleteResult DeleteAllMerchantsDailyActivity()
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);

            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var deleteResult = collection.DeleteMany(_ => _.merchant_id != 0);

            return deleteResult;
        }

        public static DeleteResult DeleteAllMerchantDailyActivity(int merchant_id)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);

            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var deleteResult = collection.DeleteMany(_ => _.merchant_id == merchant_id);

            return deleteResult;
        }

        public static DeleteResult DeleteAllMerchantDailyActivity(int merchant_id, DateTime xct_posting_date)
        {
            var client = GetMongoClient();

            var db = client.GetDatabase(DB_NAME);

            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var deleteResult = collection.DeleteOne(_ => _.merchant_id == merchant_id  && _.xct_posting_date == xct_posting_date);

            return deleteResult;
        }
    }
}
