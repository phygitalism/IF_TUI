using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hanssens.Net;

using RecognitionService.Models;

namespace RecognitionService
{
	class Storage
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
			_storage.Store(tangiblesKey, config);
			_storage.Persist();
		}

		public MarkerConfig Load()
		{
			MarkerConfig result;
			try
			{
				result = _storage.Get<MarkerConfig>(tangiblesKey);
			}
			catch (ArgumentNullException ex)
			{
				Console.WriteLine(ex);
				result = new MarkerConfig();
				Save(result);
			}
			return result;
		}
	}
}
