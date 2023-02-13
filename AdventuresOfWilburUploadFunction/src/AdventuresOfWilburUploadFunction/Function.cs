using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AdventuresOfWilburUploadFunction
{
    public class Function
    {
        private ServiceCollection _serviceCollection;

        public Function()
        {
            ConfigureServices();
        }
        
        private void ConfigureServices()
        {
            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddDefaultAWSOptions(new AWSOptions());
            _serviceCollection.AddAWSService<IAmazonDynamoDB>();
            _serviceCollection.AddAWSService<IAmazonS3>();
            _serviceCollection.AddTransient<Handler>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest input, ILambdaContext context)
        {
            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
                return await serviceProvider.GetService<Handler>().Handle(input);
        }
    }
}
