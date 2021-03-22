using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.DynamoDBEvents;
using Newtonsoft.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Assignment7
{
    public class Item
    {
        public string itemId;
        public string description;
        public string type;
        public string company;
        public int rating;
        public string lastInstanceOfWord;
    }
    public class Function
    {
        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        public async Task<List<Item>> FunctionHandler(DynamoDBEvent input, ILambdaContext context)
        {
            Table table = Table.LoadTable(client, "Assignment5A");
            List<Item> items = new List<Item>();

            List<DynamoDBEvent.DynamodbStreamRecord> records = (List<DynamoDBEvent.DynamodbStreamRecord>)input.Records;

            if(records.Count > 0)
            {
                DynamoDBEvent.DynamodbStreamRecord record = records[0];
                if(record.EventName.Equals("INSERT"))
                {
                    Document myDoc = Document.FromAttributeMap(record.Dynamodb.NewImage);
                    Item myItem = JsonConvert.DeserializeObject<Item>(myDoc.ToJson());
                    var request = new UpdateItemRequest
                    {
                        TableName = "RatingsByType",
                        Key = new Dictionary<string, AttributeValue>
                        {
                            {"type", new AttributeValue {S = myItem.type} }
                        },
                        AttributeUpdates = new Dictionary<string, AttributeValueUpdate>()
                        {
                            {
                                "numRatings",
                                new AttributeValueUpdate {Action = "ADD", Value = new AttributeValue { N = "1"} }
                            },
                            {
                                "averageRating",
                                new AttributeValueUpdate {Action = "ADD", Value = new AttributeValue{ N = "1"}}
                            },
                        },
                    };
                    await client.UpdateItemAsync(request);
                }
            }

            return items;
        }
    }
}
