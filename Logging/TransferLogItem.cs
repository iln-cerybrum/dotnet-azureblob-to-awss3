using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerybrum.ILN.app.AzureToAws.Logging
{
	public class TransferLogItem
	{
        /// <summary>
        /// the blob file name in the container, which will also be the name in the destination s3 bucket
        /// </summary>
        public required string FileName { get; set; }
    }
}
