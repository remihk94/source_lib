using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using OsmSharp.Streams;
using OsmSharp;
using res_appScript;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
public class Program
{
    /// <summary>
    /// The main entry point of the program.
    /// </summary>
    public static async Task Main()
    {
        // Prompt the user to enter the type of data injection (SQL or MongoDB)
        Console.WriteLine("Enter the type of data injection (SQL or MongoDB):");
        string injectionType = Console.ReadLine();

        // Load the OSM data from the specified file path
        var osmStreamSource = new FileInfo(Constants.OsmFilePath).OpenRead();
        var osmData = new PBFOsmStreamSource(osmStreamSource);

        if (injectionType.Equals("SQL", StringComparison.OrdinalIgnoreCase))
        {
            await InjectIntoSQL(osmData);
        }
        else if (injectionType.Equals("MongoDB", StringComparison.OrdinalIgnoreCase))
        {
            await InjectIntoMongoDB(osmData);
        }
        else
        {
            Console.WriteLine("Invalid injection type. Please choose either SQL or MongoDB.");
        }
    }

    /// <summary>
    /// Converts OSM data to a list of SqlGeometry objects representing points.
    /// </summary>
    /// <param name="data">The OSM data source.</param>
    /// <returns>The list of SqlGeometry objects.</returns>
    private static List<SqlGeometry> ConvertToGeometries(PBFOsmStreamSource data)
    {
        // Convert items to Geometry types 
        var geometries = new List<SqlGeometry>();

        foreach (var item in data.Take(Constants.DatasetSize))
        {
            if (item is Node node && node.Latitude.HasValue && node.Longitude.HasValue)
            {
                // Create a SqlGeometry point using the latitude and longitude values
                var point = SqlGeometry.Point(node.Latitude.Value, node.Longitude.Value, 0);
                geometries.Add(point);
            }
        }

        return geometries;
    }

    /// <summary>
    /// Injects the OSM data into a SQL database.
    /// </summary>
    /// <param name="data">The OSM data source.</param>
    private static async Task InjectIntoSQL(PBFOsmStreamSource data)
    {
        // Convert loaded OSM data to geometry points 
        var geometries = ConvertToGeometries(data);

        using (var connection = new SqlConnection(Constants.ConnectionString))
        {
            await connection.OpenAsync();

            // Divide the geometries into chunks
            var chunks = Chunkify(geometries, Constants.ChunkSize);

            foreach (var chunk in chunks)
            {
                // Insert each chunk into the database
                await InsertChunkIntoDatabase(chunk, connection);
                Console.WriteLine("Chunk completed!");
            }
        }
    }

    /// <summary>
    /// Divides a list of geometries into chunks of the specified size.
    /// </summary>
    /// <param name="geometries">The list of geometries.</param>
    /// <param name="chunkSize">The size of each chunk.</param>
    /// <returns>An enumerable of chunks.</returns>
    private static IEnumerable<List<SqlGeometry>> Chunkify(List<SqlGeometry> geometries, int chunkSize)
    {
        for (int i = 0; i < geometries.Count; i += chunkSize)
        {
            // Get a chunk of geometries using the GetRange method
            yield return geometries.GetRange(i, Math.Min(chunkSize, geometries.Count - i));
        }
    }

    /// <summary>
    /// Inserts a chunk of geometries into the SQL database.
    /// </summary>
    /// <param name="chunk">The chunk of geometries.</param>
    /// <param name="connection">The SQL connection.</param>
    private static async Task InsertChunkIntoDatabase(List<SqlGeometry> chunk, SqlConnection connection)
    {
        using (var command = new SqlCommand())
        {
            command.Connection = connection;
            command.CommandText = "INSERT INTO center_america(location) VALUES(geometry::STGeomFromText(@geomText, 0))";
            command.Parameters.Add("@geomText", SqlDbType.VarChar);

            foreach (var item in chunk)
            {
                // Set the parameter value for the geometry text
                command.Parameters["@geomText"].Value = item.ToString();

                // Execute the SQL command to insert the geometry
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    /// <summary>
    /// Injects the OSM data into a MongoDB database.
    /// </summary>
    /// <param name="data">The OSM data source.</param>
    private static async Task InjectIntoMongoDB(PBFOsmStreamSource data)
    {
        // Convert OSM data to GeoJSON locations
        var geolocations = ConvertToGeoJson(data);

        // Create the MongoDB client, database, and collection
        var client = new MongoClient(Constants.MongoConnect);
        var database = client.GetDatabase(Constants.MongoDatabaseName);
        var collection = database.GetCollection<Location>(Constants.MongoCollectionName);

        // Insert the geolocations into the MongoDB collection
        await collection.InsertManyAsync(geolocations);
    }

    /// <summary>
    /// Converts OSM data to a list of Location objects representing GeoJSON points.
    /// </summary>
    /// <param name="data">The OSM data source.</param>
    /// <returns>The list of Location objects.</returns>
    private static List<Location> ConvertToGeoJson(PBFOsmStreamSource data)
    {
        var locations = new List<Location>();

        foreach (var item in data.Take(Constants.DatasetSize))
        {
            if (item is Node node && node.Latitude.HasValue && node.Longitude.HasValue)
            {
                // Create a Location object with a GeoJSON point
                var location = new Location
                {
                    LocationPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                        new GeoJson2DGeographicCoordinates(node.Longitude.Value, node.Latitude.Value))
                };
                locations.Add(location);
            }
        }

        return locations;
    }
}