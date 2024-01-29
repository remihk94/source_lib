using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace res_appScript
{
    public class Location
    {
        [BsonId] // Indicates that this property is the identifier for the document in the MongoDB collection
        public ObjectId Id { get; set; } 

        [BsonElement("location")] // Specifies the mapping to the "location" field in the MongoDB 
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> LocationPoint { get; set; }
    }
}
