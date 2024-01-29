using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace res_appScript
{
    public class Constants
    {
        /// <summary>
        /// The connection string for the database.
        /// </summary>
        public const string ConnectionString = "CONNECTION STRING HERE ..";

        /// <summary>
        /// The size of the dataset to load from the OSM file.
        /// </summary>
        public const int DatasetSize = 10000; // DATA SET SIZE HERE 

        /// <summary>
        /// The size of each chunk for batch processing.
        /// </summary>
        public const int ChunkSize = 1000; // CHUNK SIZE HERE 


        /// <summary>
        /// The path to the OSM file.
        /// </summary>
        public const string OsmFilePath = "DATA SET PATH HERE ..";

        /// <summary>
        /// The mongodb connection string.
        /// </summary>
        public const string MongoConnect = "MONGO DB CONNECTION STRING HERE ..";

        /// <summary>
        /// The mongodb database name.
        /// </summary>
        public const string MongoDatabaseName = "MONGO DB NAME HERE";

        /// <summary>
        /// The mongodb collection name.
        /// </summary>
        public const string MongoCollectionName = "MONGODB COLLECTION NAME HERE";
    }
}