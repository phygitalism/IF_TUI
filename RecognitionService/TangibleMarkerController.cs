using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RecognitionService.Models;
using RecognitionService.Services;

using log4net;
using log4net.Config;

namespace RecognitionService
{
	public class TangibleMarkerController : IDisposable
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(TangibleMarkerController));
		public MarkerConfig Config { get; private set; }

		private JsonStorage<MarkerConfig> _storage = new JsonStorage<MarkerConfig>("markers");

		public TangibleMarkerController()
		{
			Config = _storage.Load();
		}

		public void RegisterMarkerWithId(int id, (Vector2 v1, Vector2 v2, Vector2 v3) vertexes)
		{
			Logger.Info($"RegisterMarkerWithId {id}: {vertexes}");
			var tangible = new RegistredTangibleMarker(id, vertexes);
			if (!Config.IsRegistredWithId(tangible.Id))
			{
				Config.Add(tangible);
			}
			else
			{
				Logger.Info($"Already registered marker with id {id}. Update {vertexes}");
				Config.Update(tangible);
			}
			_storage.Save(Config);
		}

		public void UnregisterMarkerWithId(int id)
		{
			Logger.Info($"UnregisterMarkerWithId {id}");
			if (!Config.IsRegistredWithId(id)) { return; }

			var tangible = Config.GetTangibleWithId(id);
			Config.Remove(tangible);
			_storage.Save(Config);
		}

		public int[] GetAllRegistredIds()
		{
			Logger.Info("OnMarkerListRequested");
			return Config.registredTangibles.Select(marker => marker.Id).ToArray();
		}

		public void Dispose()
		{
			_storage.Dispose();
		}
	}
}
