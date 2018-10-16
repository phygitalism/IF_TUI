using System;
using System.IO;

using Newtonsoft.Json;
using RecognitionService.Models;

namespace RecognitionService
{
	class Storage : IDisposable
	{
		private const string storageName = "config" + ".localstorage";

		public void Save(MarkerConfig config)
		{
			var json = JsonConvert.SerializeObject(config, Formatting.Indented);
			File.WriteAllText(storageName, json);
		}

		public MarkerConfig Load()
		{
			MarkerConfig result;
			try
			{
				var json = File.ReadAllText(storageName);
				result = JsonConvert.DeserializeObject<MarkerConfig>(json);
			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine(ex);
				result = new MarkerConfig();
				Save(result);
			}
			return result;
		}

		public void Dispose()
		{

		}
	}
}
