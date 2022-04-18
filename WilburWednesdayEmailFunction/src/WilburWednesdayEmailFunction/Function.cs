using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using System.Text.Json;
using Amazon.SimpleEmail;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleEmail.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace WilburWednesdayEmailFunction;

public class Function
{
    IAmazonS3 S3Client { get; set; }
    IAmazonSimpleEmailService SESClient { get; set; }
    IAmazonSimpleSystemsManagement SimpleSystemsManagementClient { get; set; }
    private static readonly HttpClient _httpClient = new HttpClient();
    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        S3Client = new AmazonS3Client();
        SESClient = new AmazonSimpleEmailServiceClient();
        SimpleSystemsManagementClient = new AmazonSimpleSystemsManagementClient();
    }

    /// <summary>
    /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
    /// </summary>
    /// <param name="s3Client"></param>
    public Function(IAmazonS3 s3Client, IAmazonSimpleEmailService sesClient)
    {
        this.S3Client = s3Client;
        this.SESClient = sesClient;
    }
    
    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<string?> FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        Console.WriteLine("Hello world");
        Console.WriteLine(JsonSerializer.Serialize(evnt));

        var s3Event = evnt.Records?[0].S3;
        if(s3Event == null)
        {
            return null;
        }

        try
        {
            var response = await this.S3Client.GetObjectMetadataAsync(s3Event.Bucket.Name, s3Event.Object.Key);
            Console.WriteLine(JsonSerializer.Serialize(await GetLatestWilburWednesday()));
            var latestWilbur = await GetLatestWilburWednesday();
            await SendEmail(new EmailItem(latestWilbur));
            return response.Headers.ContentType;
        }
        catch(Exception e)
        {
            context.Logger.LogInformation($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
            context.Logger.LogInformation(e.Message);
            context.Logger.LogInformation(e.StackTrace);
            throw;
        }
    }

    public async Task SendEmail(EmailItem email)
    {
        var sendRequest = new SendEmailRequest
        {
            Source = await GetSenderAddress(),
            Destination = new Destination
            {
                ToAddresses = await GetToAddresses()
            },
            Message = new Message
            {
                Subject = new Content(email.Subject),
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = email.HtmlBody
                    },
                    Text = new Content
                    {
                        Charset = "UTF-8",
                        Data = email.TextBody
                    }
                }
            }
        };

        try
        {
            Console.WriteLine("Sending email using SES");
            Console.WriteLine("Successfully sent");
        } catch (Exception ex)
        {
            Console.WriteLine("Failed to send: " + ex);
        }
    }

    private async Task<List<string>> GetToAddresses()
    {
        var toAddressesResponse = await SimpleSystemsManagementClient.GetParameterAsync(new Amazon.SimpleSystemsManagement.Model.GetParameterRequest
        {
            Name = "wilburEmailToAddresses",
            WithDecryption = false
        });

        var toAddressesString = toAddressesResponse.Parameter.Value;
        Console.WriteLine(JsonSerializer.Serialize(toAddressesString.Split(",").Select(x => x.Trim()).ToList()));
        return toAddressesString.Split(",").Select(x => x.Trim()).ToList();
    }

    private async Task<string> GetSenderAddress()
    {
        var fromAddressesResponse = await SimpleSystemsManagementClient.GetParameterAsync(new Amazon.SimpleSystemsManagement.Model.GetParameterRequest
        {
            Name = "wilburEmailFromAddresses",
            WithDecryption = false
        });

        return fromAddressesResponse.Parameter.Value;
    }

    private async Task<WilburWednesdayPost> GetLatestWilburWednesday()
    {
        var response = await _httpClient.GetStreamAsync("https://api.joshuarichardson.dev/WilburLatest");
        var wilburLatest = JsonSerializer.Deserialize<WilburWednesdayPost>(response);
        
        if (wilburLatest == null)
            return new WilburWednesdayPost();

        return wilburLatest;
    }
}