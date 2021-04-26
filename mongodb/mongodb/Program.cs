using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Dynamic;


namespace mongodb
{
    #region librairie mongodb
    /*
     * @{desc}
     */
    struct dbConf
    {
        public dbConf(string client = null, string dbName = null)
        {
            this.client = client;
            this.dbName = dbName;
        }

        public string client;
        public string dbName;
    }

    /*
     * @{desc}
     */
    class DB
    {

        public dbConf conf;
        public MongoClient client = null;
        public string db = null;
        public string collection = null;

        public DB(string client = "localhost", string dbName = null, string collName = null)
        {
            this.conf = new dbConf(client, dbName);
            this.Client(client);
            if (!string.IsNullOrEmpty(dbName)) this.Database(dbName);
            if (!string.IsNullOrEmpty(collName)) this.Collection(collName);
        }

        void Client(string client = null)
        {
            this.client = new MongoClient(client);
        }

        void Database(string dbName = null)
        {
            this.db = dbName;
        }

        void Collection(string collName = null)
        {
            this.collection = collName;
        }

        public dynamic Find(ExpandoObject querry = null)
        {

            foreach (KeyValuePair<string, object> kvp in ((IDictionary<string, object>)querry))
            {
                string PropertyWithValue = kvp.Key + ": " + kvp.Value.ToString();
                var database = this.client.GetDatabase(this.db);
                var collection = database.GetCollection<BsonDocument>(this.collection);
                return collection.Find(Builders<BsonDocument>.Filter.Eq(kvp.Key, kvp.Value.ToString())).ToList();
            }
            return false;
        }

        public bool Insert(ExpandoObject querry = null)
        {

            try
            {
                var database = this.client.GetDatabase(this.db);
                var collection = database.GetCollection<BsonDocument>(this.collection);
                collection.InsertOne(new BsonDocument(querry));
                return true;
            }
            catch (MongoWriteException e)
            {
                Console.WriteLine(e);
                return false;
            }

        }

        public dynamic UpdateOne(FilterDefinition<BsonDocument> filtre, UpdateDefinition<BsonDocument> update)
        {
            var database = this.client.GetDatabase(this.db);
            var collection = database.GetCollection<BsonDocument>(this.collection);
            return collection.UpdateOne(filtre, update).IsAcknowledged;
        }

        public dynamic Delete(FilterDefinition<BsonDocument> filtre)
        {
            var database = this.client.GetDatabase(this.db);
            var collection = database.GetCollection<BsonDocument>(this.collection);
            return collection.DeleteOne(filtre).IsAcknowledged;
        }

        public void ShowCollections()
        {
            foreach (var e in this.CollectionsNames())
            {
                Console.WriteLine(e);
            }
        }

        public dynamic CollectionsNames()
        {
            var database = this.client.GetDatabase(this.db);
            return database.ListCollectionsAsync().Result.ToListAsync<BsonDocument>().Result;
        }

    }
    #endregion

    #region partie de test
    class Program
    {

        static void Main(string[] args)
        {
            // création du client
            DB dataStorage = new DB("mongodb+srv://<user>:<mdp>@cluster0.xgpkc.mongodb.net/EXPREDITOR?retryWrites=true&w=majority", "faceid", "test");

            
            // exemple querry find()
            dynamic find = new ExpandoObject();
            find.name = "test";
            dynamic results = dataStorage.Find(find);
            foreach (var r in results)
            {
                Console.WriteLine(r);
            }

            // exemple querry insert()
            dynamic insert = new ExpandoObject();
            insert.name = "premierTest";
            dataStorage.Insert(insert);

            // exemple querry update()
            FilterDefinition<BsonDocument> filterDefinition = Builders<BsonDocument>.Filter.Eq("name", "premierTest");
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Set("name", "loool");
            results = dataStorage.UpdateOne(filterDefinition, update);
            Console.WriteLine(results);

            // exemple querry delete()

            var delete = Builders<BsonDocument>.Filter.Eq("name", "loool");
            results = dataStorage.Delete(delete);
            Console.WriteLine(results);

            /*Console.WriteLine(dataStorage.CollectionsNames());*/

            // exemple d'obtention et d'affichage des collections
            /*dynamic collections = dataStorage.CollectionsNames();*/
            /*dataStorage.ShowCollections();*/

            Console.ReadKey();

        }
    }
    #endregion
}
