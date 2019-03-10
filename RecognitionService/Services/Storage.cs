using System;
using System.IO;

using Newtonsoft.Json;
using RecognitionService.Models;

namespace RecognitionService.Services
{
	public class JsonStorage<T> : IDisposable where T : new()
	{
		private string _defaultFileExtension = "json";
		private string _storageName;

		public string StorageName
		{
			get { return $"{_storageName}.{_defaultFileExtension}"; }
		}

		public JsonStorage(string storageName)
		{
			this._storageName = storageName;
		}

		public void Save(T data)
		{
			var json = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(StorageName, json);
		}

		public T Load()
		{
			T result;
			try
			{
				var json = File.ReadAllText(StorageName);
				result = JsonConvert.DeserializeObject<T>(json);
			}
			catch (FileNotFoundException ex)
			{
				result = new T();
				Save(result);
			}
			return result;
		}

		public void Dispose()
		{

		}
	}
}
