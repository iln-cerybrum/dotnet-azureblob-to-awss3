using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Cerybrum.ILN.app.AzureToAws.Config;
using Cerybrum.ILN.app.AzureToAws.ExtensionMethods;

namespace Cerybrum.ILN.app.AzureToAws.Logging
{
	/// <summary>
	/// this is used to track transfers locally when <see cref="TransferBehaviorConfig.TrackTransfersInLocalLogs"/> is true. It allows us to pick up where we left off if any issues
	/// </summary>
	public class TransferLog
	{
        /// <summary>
        /// This is a unique key which is the combined value of the <see cref="AwsConfig"/> and <see cref="AzureConfig"/> which includes a hashed value of their settings. If a new run is made, it will check the latest log 
        /// </summary>
        public required string TransferKey { get; set; }

		/// <summary>
		/// This is the last time all blobs were retrieved. If the app starts over again, it will only retrive blobs after this date
		/// </summary>
		public DateTimeOffset? LastRetrievalDate { get; set; }

		/// <summary>
		/// holds the absolute path to the file name 
		/// </summary>
		[JsonIgnore]
		public string FileName { get; set; }

		/// <summary>
		/// names of blob objects that have not yet been processed
		/// </summary>
        public List<string> PendingTransfers { get; set; } = new List<string>();
        
		/// <summary>
		/// names of blob objects that have completed
		/// </summary>
		public List<string> CompletedTransfers { get; set; } = new List<string>();
        
		/// <summary>
		/// names of blob objects that have failed. Check console errors
		/// </summary>
		public List<string> FailedTransfers { get; set; } = new List<string>();



		/// <summary>
		/// Loads the log from the file. If it doesn't exist, creates one
		/// </summary>
		/// <param name="transferKey">The unique key of this transfer, which is combined using config settings. If there is a log that exists but the transfer key provided does not match <see cref="TransferKey"/> of the log, it will clear the log and start new</param>
		/// <param name="logFileName">The file name (e.g. transfer.log)</param>
		/// <returns></returns>
		public static async Task<TransferLog> LoadAsync(string transferKey, string logFileName)
		{
			TransferLog? log;
			var fullFileName = Path.Combine(Directory.GetCurrentDirectory(), logFileName);

			if (File.Exists(fullFileName))
			{
				using (var fileStream = File.OpenRead(fullFileName))
				{
					log = await JsonSerializer.DeserializeAsync<TransferLog>(fileStream);
				}

				if (log?.TransferKey == transferKey)
				{
					//this is a previous log, so we'll start again
					log.FileName = fullFileName;

					return log!;
				}
			}

			//file doesn't exist OR transfer key doesn't match. Create a new log
			log = new TransferLog() { TransferKey = transferKey, FileName = fullFileName };

			//save the log
			await log.SaveAsync();

			return log;
		}//end LoadAsync


		/// <summary>
		/// this is when <see cref="TransferBehaviorConfig.TrackTransfersInLocalLogs"/> is false
		/// </summary>
		/// <returns></returns>
		public static TransferLog Fake()
			=> new TransferLog()
			{
				TransferKey = "DONTSAVE",
				FileName = string.Empty
			};

		public async Task SaveAsync()
		{
			await ThreadingExtensions.DoWithLockAsync(async () =>
			{
				//this will overwrite the old file. This can be improved to be atomic
				using (var fileStream = File.Create(FileName))
				{
					await JsonSerializer.SerializeAsync(fileStream, this);
				}
			});
		}//end SaveAsync
	}
}
