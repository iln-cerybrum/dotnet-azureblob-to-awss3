using Cerybrum.ILN.app.AzureToAws;
using Cerybrum.ILN.app.AzureToAws.Config;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

// Bind configurations
var azureConfig = config.GetRequiredSection("azure").Get<AzureConfig>();
var awsConfig = config.GetRequiredSection("aws").Get<AwsConfig>();
var transferBehavior = config.GetRequiredSection("transferBehavior").Get<TransferBehaviorConfig>();

// Create an instance of the service and run it
var service = new DataTransferService(awsConfig!, azureConfig!, transferBehavior!);

await service.ExecuteTransfers();

Console.ReadKey();