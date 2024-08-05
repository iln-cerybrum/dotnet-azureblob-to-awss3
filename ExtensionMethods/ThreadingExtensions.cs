using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerybrum.ILN.app.AzureToAws.ExtensionMethods;

public static class ThreadingExtensions
{
	static readonly Semaphore semaphore = new(1, 1);



	/// <summary>
	/// Since we're processing in parallel, we use this to lock when doing actions that can interfere
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public static async Task DoWithLockAsync(this Func<Task> action)
	{
		try
		{
			semaphore.WaitOne();

			await action();
		}
		finally
		{
			semaphore.Release();
		}
	}
}
