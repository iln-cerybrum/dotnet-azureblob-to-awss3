using Cerybrum.ILN.app.AzureToAws.Config;
using Cerybrum.ILN.app.AzureToAws.ExtensionMethods;
using Cerybrum.ILN.app.AzureToAws.Logging;
using Cerybrum.ILN.app.AzureToAws.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cerybrum.ILN.app.AzureToAws
{
	public class DataTransferService
	{
		#region fields

		private AwsConfig _awsConfig;
		private AzureConfig _azureConfig;
		private AzureBlobService _azureSvc;
		private TransferBehaviorConfig _transferBehavior;
		static Stopwatch _stopwatch = new Stopwatch();

		#endregion



		#region ctor

		public DataTransferService(AwsConfig awsConfig, AzureConfig azureConfig, TransferBehaviorConfig transferBehavior)
		{
			this._awsConfig = awsConfig;
			this._azureConfig = azureConfig;
			this._azureSvc = new AzureBlobService(azureConfig);
			this._transferBehavior = transferBehavior;
		}

		#endregion



		#region methods

		public async Task ExecuteTransfers()
		{
			_stopwatch.Start();

			TransferLog transferLog = await RetrieveOrCreateLogs();

			await LoadPendingBlobsAndSaveCheckpoint(transferLog);
			await ExecutePendingTransfers(transferLog);

			_stopwatch.Stop();
		}//end ExecuteTransfers


		private async Task ExecutePendingTransfers(TransferLog transferLog)
		{
			//grab a snapshot of this as we'll be modifying the original collection
			var allPendingTransfers = transferLog.PendingTransfers.ToArray();
			var parallelOpts = new ParallelOptions()
			{
				MaxDegreeOfParallelism = _transferBehavior.MaxTransfersAtATime
			};

			await Parallel.ForEachAsync(allPendingTransfers, parallelOpts, async (blobName, ct) =>
			{
				var awsTransfer = new AwsS3ItemTransferService(_awsConfig);

				try
				{
					await awsTransfer.TransferAsync(
						blobName,
						await _azureSvc.GetStream(blobName),
						_transferBehavior.OverwriteFilesIfExists);

					await DoOnTransferSuccess(transferLog, blobName);
				}
				catch (Exception ex)
				{
					await DoOnTransferError(transferLog, ex, blobName);
				}
			});

			await Console.Out.WriteLineAsync($"Transfers completed! Success: {transferLog.CompletedTransfers.Count}; Failures: {transferLog.FailedTransfers.Count}; Total Time: {_stopwatch.Elapsed}");
		}//end ExecutePendingTransfers


		async Task DoOnTransferError(TransferLog transferLog, Exception ex, string blobName)
		{
			await ThreadingExtensions.DoWithLockAsync(async () =>
			{
				await Console.Error.WriteLineAsync($"Error transferring blob {blobName}");
				await Console.Error.WriteLineAsync(ex.Message);

				transferLog.PendingTransfers.Remove(blobName);
				transferLog.FailedTransfers.Add(blobName);
			});

			if (_transferBehavior.TrackTransfersInLocalLogs)
				await transferLog.SaveAsync();
		}


		async Task DoOnTransferSuccess(TransferLog transferLog, string blobName)
		{
			await ThreadingExtensions.DoWithLockAsync(async () =>
			{
				transferLog.PendingTransfers.Remove(blobName);
				transferLog.CompletedTransfers.Add(blobName);

				await Console.Out.WriteLineAsync($"Success: {blobName} | {transferLog.CompletedTransfers.Count} Completed | {transferLog.PendingTransfers.Count} Remaining | Elapsed: {_stopwatch.Elapsed}");
			});

			if (_transferBehavior.TrackTransfersInLocalLogs)
				await transferLog.SaveAsync();
		}


		private async Task LoadPendingBlobsAndSaveCheckpoint(TransferLog transferLog)
		{
			//load everything that needs to be transferred, starting from last date until now
			DateTimeOffset lastLoadDate = transferLog.LastRetrievalDate ?? DateTimeOffset.MinValue;
			//now is the marker. If we run this again using the same values and with the log save, it'll pick up where it left off using this date as the marker
			DateTimeOffset nowDate = DateTimeOffset.UtcNow;

			//load up all the blobs
			await foreach (var blob in _azureSvc.LoadBlobsAsync(lastLoadDate, nowDate))
			{
				transferLog.PendingTransfers.Add(blob);
			}

			//save the checkpoint in case we run this again (like tomorrow)
			transferLog.LastRetrievalDate = nowDate;

			if (_transferBehavior.TrackTransfersInLocalLogs)
				await transferLog.SaveAsync();
		}//end LoadPendingBlobs


		private async Task<TransferLog> RetrieveOrCreateLogs()
		{
			var transferKey = GetTransferKey();

			return _transferBehavior.TrackTransfersInLocalLogs
				? await TransferLog.LoadAsync(transferKey, _transferBehavior.LogFileName)
				: TransferLog.Fake();
		}


		string GetTransferKey()
		{
			var transferKey = $"{_awsConfig.ToString()}::{_azureConfig.ToString()}";

			//encrypt the string for security purposes
			using (var sha256Hash = SHA256.Create())
			{
				// ComputeHash - returns byte array  
				byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(transferKey));
				// Convert byte array to a string   
				var builder = new StringBuilder();

				for (int i = 0; i < bytes.Length; i++)
				{
					builder.Append(bytes[i].ToString("x2"));
				}

				return builder.ToString();
			}
		}

		#endregion methods
	}
}
