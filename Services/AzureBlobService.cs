using Cerybrum.ILN.app.AzureToAws.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Storage.Blobs;
using Microsoft.VisualBasic;
using Azure.Storage.Blobs.Specialized;

namespace Cerybrum.ILN.app.AzureToAws.Services;

/// <summary>
/// loads all the blobs in azure storage
/// </summary>
public class AzureBlobService
{
	private readonly AzureConfig _config;



	public AzureBlobService(AzureConfig config)
    {
		this._config = config;
	}



    /// <summary>
    /// Gets all the blobs created after and before the specified dates
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<string> LoadBlobsAsync(DateTimeOffset createdAfter, DateTimeOffset createdBefore)
    {
		var client = new BlobContainerClient(_config.StorageConnectionString, _config.ContainerName);

		//this gets all blogs
		await foreach (var item in client.GetBlobsAsync())
		{
			var created = item.Properties.CreatedOn ?? DateTimeOffset.MinValue.AddDays(1);

			if (created > createdAfter && created < createdBefore)
				yield return item.Name;
		}
	}//end GetBlobs


	/// <summary>
	/// gets the blob stream so that it can be uploaded to s3
	/// </summary>
	/// <param name="blobName"></param>
	/// <returns></returns>
	public async Task<Stream> GetStream(string blobName)
	{
		var client = new BlobContainerClient(_config.StorageConnectionString, _config.ContainerName);

		return await client.GetBlobClient(blobName).OpenReadAsync();
	}

}
