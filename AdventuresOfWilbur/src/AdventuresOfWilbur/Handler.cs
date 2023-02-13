﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace AdventuresOfWilbur
{
    public class Handler
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        
        private const string BucketBaseUrl = "https://adventures-of-wilbur-images.s3.eu-west-2.amazonaws.com";

        public Handler(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest input)
        {
            try
            {
                var storyItem = int.Parse(input.QueryStringParameters["storyItemNumber"]);
                var imageTime = input.QueryStringParameters["imageTime"];

                Console.WriteLine(storyItem);
                
                var tableDescription = new DescribeTableRequest
                {
                    TableName = "AdventuresOfWilburImageTable"
                };

                
                Console.WriteLine("Describing");
                var description = await _dynamoDb.DescribeTableAsync(tableDescription);
                Console.WriteLine("Yay");
                int numberOfItems = int.Parse(description.Table.ItemCount.ToString());
                Console.WriteLine($"Item count: {numberOfItems}");

                int imageId = 1;
                switch (imageTime)
                {
                    case "latest":
                        imageId = numberOfItems;
                        break;
                    case "random":
                        var rand = new Random();
                        imageId = rand.Next(1, (numberOfItems + 1));
                        break;
                    default:
                        imageId = storyItem;
                        break;
                }
                
                var getRequest = new GetItemRequest
                {
                    TableName = "AdventuresOfWilburImageTable",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        {"ImageId", new AttributeValue{ N = imageId.ToString() }}
                    },
                    ProjectionExpression = "#title, #description, #imageKey, #friends",
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        {"#imageKey", "ImageKey"},
                        {"#title", "Title"},
                        {"#description", "Description"},
                        {"#friends", "Friends"}
                    },
                };

                var response = await _dynamoDb.GetItemAsync(getRequest);
                
                if(!response.IsItemSet)
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 403,
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                        Body = ""
                    };

                var imageName = response.Item["ImageKey"].S;
                var imageTitle = response.Item["Title"].S;
                var imageDescription = response.Item["Description"].S;
                var imageFriends = response.Item["Friends"].SS;

                Console.WriteLine(imageName);
                Console.WriteLine(imageTitle);
                Console.WriteLine(imageDescription);
                Console.WriteLine(imageFriends);
                
                var imageUrl = $"{BucketBaseUrl}/{imageName}";
                
                var imageData = new ImageData(imageUrl, imageTitle, imageDescription, imageFriends, numberOfItems);
                var serializedImageData = JsonConvert.SerializeObject(imageData);
                
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                    Body = serializedImageData
                };
            }
            catch(KeyNotFoundException)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 403,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                    Body = ""
                };
            }
        }
    }
}