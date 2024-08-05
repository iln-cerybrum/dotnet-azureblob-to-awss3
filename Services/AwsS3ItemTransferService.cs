using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Cerybrum.ILN.app.AzureToAws.Config;

namespace Cerybrum.ILN.app.AzureToAws.Services;

/// <summary>
/// executes a transfer of an item to an S3 bucket
/// </summary>
internal class AwsS3ItemTransferService
{
	private AmazonS3Client _client;
	private AwsConfig _config;



	public AwsS3ItemTransferService(AwsConfig config)
    {
        var awsRegion = RegionEndpoint.GetBySystemName(config.Region);

		this._config = config;
        this._client = new AmazonS3Client(config.AccessKey, config.SecretKey, awsRegion);
	}



	/// <summary>
	/// Executes the transfer. If there were any issues, will 
	/// </summary>
	/// <param name="blobName"></param>
	/// <param name="stream"></param>
	/// <param name="overwrite">if true, will overwrite the current file if it exists</param>
	/// <returns></returns>
    public async Task TransferAsync(string blobName, Stream stream, bool overwrite)
	{
		TransferUtility transferUtility = new TransferUtility(_client);

		if (!overwrite)
		{
			//must check to see if the file already exists
			if (await DoesObjectExist(blobName))
				return;//file exists... do not overwrite
		}

		//construct the final key based on the folder, if provided
		var finalKey = $"{_config.FolderPath ?? string.Empty}{blobName}";

		await transferUtility.UploadAsync(new TransferUtilityUploadRequest()
		{
			BucketName = _config.Bucket,
			Key = finalKey,
			InputStream = stream,
			AutoCloseStream = true,
			StorageClass = S3StorageClass.FindValue(_config.StorageClass)
		});
	}//end Transfer


	public async Task<bool> DoesObjectExist(string key)
	{
		try
		{
			// Attempt to retrieve the object metadata
			var metadataResponse = await _client.GetObjectMetadataAsync(new GetObjectMetadataRequest
			{
				BucketName = _config.Bucket,
				Key = key
			});

			return true; // Object exists
		}
		catch (AmazonS3Exception e)
		{
			// Check if the exception is because the object does not exist
			if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				return false;

			// Re-throw the exception if it's due to another reason
			throw;
		}
	}
}
