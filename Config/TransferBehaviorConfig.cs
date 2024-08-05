using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerybrum.ILN.app.AzureToAws.Config
{
	public class TransferBehaviorConfig
	{
		public bool OverwriteFilesIfExists { get; set; } = false;
		public bool TrackTransfersInLocalLogs { get; set; } = true;
        public required string LogFileName { get; set; }
		public int MaxTransfersAtATime { get; set; } = 10;
	}
}
