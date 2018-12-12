using System;
using System.Collections.Generic;
using System.Linq;
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
        const string SERVER_NAME = @"localhost";
        const int PORT_NUMBER = 27017;
        const string DB_NAME = @"gfos2";

        internal static (string SERVER_NAME, int DB_PORT_NUMBER, string DB_NAME) GetMongoDBConnectionInfo()
        {
            // normally I would read config from a app config file
            string serverName = SERVER_NAME;
            int dbPortNumber = PORT_NUMBER;
            string dbName = DB_NAME;

            return (serverName, dbPortNumber, dbName);
        }

        public static void BootstrapMongoSchema()
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            // will create the DB if it does not exist
            var db = client.GetDatabase(dbName);

            #region ConfigDataMBE
            // will create the collection if it does not exist
            var configData = db.GetCollection<ConfigDataItemMBE>(ConfigDataItemMBE.COLLECTION_NAME);

            // create index on StdRequestTypeMBE
            configData.Indexes.CreateOne(Builders<ConfigDataItemMBE>.IndexKeys.Ascending(_ => _.name), new CreateIndexOptions() { Unique = true });
            #endregion

            #region MerchantMBE
            // will create the collection if it does not exist
            var merchants = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            // create index on StdRequestTypeMBE
            merchants.Indexes.CreateOne(Builders<MerchantMBE>.IndexKeys.Ascending(_ => _.merchant_id), new CreateIndexOptions() { Unique = true });
            #endregion

            #region MerchantDailyActivityMBE
            // will create the collection if it does not exist
            var merchantDailyActivity = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            // create index on StdHierRequestTypeMBE
            merchantDailyActivity.Indexes.CreateOne(Builders<MerchantDailyActivityMBE>.IndexKeys.Combine(
                    Builders<MerchantDailyActivityMBE>.IndexKeys.Ascending(f => f.merchant_id),
                    Builders<MerchantDailyActivityMBE>.IndexKeys.Ascending(f => f.xct_posting_date)
                ),
                new CreateIndexOptions() { Unique = true });

            #endregion
        }

        // ==== ConfigDataMBE ====================================
        public static List<ConfigDataItemMBE> GetAllConfigData()
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
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
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
            var collection = db.GetCollection<ConfigDataItemMBE>(ConfigDataItemMBE.COLLECTION_NAME);

            collection.InsertOne(new ConfigDataItemMBE() { name = configItemName, value = configItemData });
        }

        public static DeleteResult DeleteAllConfigData()
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);

            var collection = db.GetCollection<ConfigDataItemMBE>(ConfigDataItemMBE.COLLECTION_NAME);

            var filter = new BsonDocument();
            var deleteResult = collection.DeleteMany(filter);

            return deleteResult;
        }

        // ==== MerchantMBE ====================================
        public static MerchantMBE FindMerchantById(int merchant_id)
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            var filter = Builders<MerchantMBE>.Filter.Where(_ => _.merchant_id == merchant_id);

            var requestInstance = collection.Find(filter).First();

            return requestInstance;
        }

        public static MerchantMBE FindMerchantByPrimaryContactPhoneNo(string phone_no)
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            var filter = Builders<MerchantMBE>.Filter.Where(_ => _.primary_contact.phone_no == phone_no);

            var requestInstance = collection.Find(filter).First();

            return requestInstance;
        }

        public static void InsertMerchant(MerchantMBE merchant)
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            collection.InsertOne(merchant);
        }

        public static void UpdateMerchant(MerchantMBE merchant)
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            var filter = new BsonDocument("_id", merchant.ID);
            collection.ReplaceOne(filter, merchant);
        }

        public static DeleteResult DeleteMerchant(int merchant_id)
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);

            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            var deleteResult = collection.DeleteOne(_ => _.merchant_id == merchant_id);

            return deleteResult;
        }

        public static DeleteResult DeleteAllMerchants()
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);

            var collection = db.GetCollection<MerchantMBE>(MerchantMBE.COLLECTION_NAME);

            var deleteResult = collection.DeleteMany(_ => _.merchant_id != 0);

            return deleteResult;
        }

        // ==== MerchantDailyActivityMBE ========================
        public static MerchantDailyActivityMBE FindMerchantDailyActivity(int merchant_id, DateTime xct_posting_date)
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
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
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            collection.InsertOne(merchantDailyActivity);
        }

        public static void UpdateMerchantDailyActivity(MerchantDailyActivityMBE merchantDailyActivity)
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var filter = new BsonDocument("_id", merchantDailyActivity.ID);
            collection.ReplaceOne(filter, merchantDailyActivity);
        }

        // this one is thread safe daily_transactions
        public static void UpsertMerchantDailyActivity(int merchant_id, DateTime xct_posting_date, List<TransactionMBE> transactions)
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
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
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);
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

        public static DeleteResult DeleteAllMerchantDailyActivity()
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);

            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var deleteResult = collection.DeleteMany(_ => _.merchant_id != 0);

            return deleteResult;
        }

        public static DeleteResult DeleteAllMerchantDailyActivity(int merchant_id)
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);

            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var deleteResult = collection.DeleteMany(_ => _.merchant_id == merchant_id);

            return deleteResult;
        }

        public static DeleteResult DeleteAllMerchantDailyActivity(int merchant_id, DateTime xct_posting_date)
        {
            (string serverName, int portNumber, string dbName) = GetMongoDBConnectionInfo();

            var client = new MongoClient($"mongodb://{serverName}:{portNumber}");

            var db = client.GetDatabase(dbName);

            var collection = db.GetCollection<MerchantDailyActivityMBE>(MerchantDailyActivityMBE.COLLECTION_NAME);

            var deleteResult = collection.DeleteOne(_ => _.merchant_id == merchant_id  && _.xct_posting_date == xct_posting_date);

            return deleteResult;
        }
    }
}
