using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerybrum.ILN.app.AzureToAws.Config
{
	public class AzureConfig
	{
        public required string StorageConnectionString { get; set; }
        public required string ContainerName { get; set; }



		public override string ToString()
		{
			return $"{StorageConnectionString}::{ContainerName}";
		}
	}
}
