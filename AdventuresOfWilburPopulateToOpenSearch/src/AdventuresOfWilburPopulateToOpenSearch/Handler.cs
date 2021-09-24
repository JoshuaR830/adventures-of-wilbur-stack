using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.OpenSearchService;
using Amazon.OpenSearchService.Model;

namespace AdventuresOfWilburPopulateToOpenSearch
{
    public class Handler
    {
        private IAmazonDynamoDB _dynamo;
        private IAmazonOpenSearchService _search;

        public Handler(IAmazonDynamoDB dynamo, IAmazonOpenSearchService search)
        {
            _dynamo = dynamo;
            _search = search;
        }

        public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest input)
        {
            var scanResult = await _dynamo.ScanAsync(new ScanRequest {TableName = "AdventuresOfWilburImageTable"});



            foreach (var item in scanResult.Items)
            {
                
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = input.Body
            };
        }
    }
}