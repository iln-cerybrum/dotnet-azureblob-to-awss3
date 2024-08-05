using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerybrum.ILN.app.AzureToAws.Config
{
	public class AwsConfig
	{
		public required string AccessKey { get; set; }
        public required string SecretKey { get; set; }
        public required string Bucket { get; set; }
		public required string Region { get; set; }
        public string? FolderPath { get; set; }
		public required string StorageClass { get; set; } = S3StorageClass.Standard.Value; 



		public override string ToString()
		{
			return $"{AccessKey}::{SecretKey}::{Bucket}::{FolderPath}::{Region}::{StorageClass}";
		}
	}
}
