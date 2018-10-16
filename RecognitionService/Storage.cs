using System;
using System.IO;

using Hanssens.Net;

using RecognitionService.Models;

namespace RecognitionService
{
	class Storage : IDisposable
	{
		private const string tangiblesKey = "tangibles";
		private const string storageName = "config" + ".localstorage";

		private LocalStorage _storage;

		public Storage()
		{
			var basePath = Environment.CurrentDirectory;
			var filePath = Path.Combine(basePath, "Resources", storageName);
			var config = new LocalStorageConfiguration()
			{
				AutoLoad = true,
				AutoSave = true,
				Filename = storageName
			};

			_storage = new LocalStorage(config);
		}

		public void Save(MarkerConfig config)
		{
			var json = Serialization.SerializeConfig(config);
			_storage.Store(tangiblesKey, json);
			_storage.Persist();
		}

		public MarkerConfig Load()
		{
			MarkerConfig result;
			try
			{
				var json = _storage.Get<string>(tangiblesKey);
				result = Serialization.DeserializeConfig(json);
			}
			catch (ArgumentNullException ex)
			{
				Console.WriteLine(ex);
				result = new MarkerConfig();
				Save(result);
			}
			return result;
		}

		public void Dispose()
		{
			_storage.Dispose();
		}
	}
}
