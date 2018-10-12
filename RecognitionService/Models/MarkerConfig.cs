using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RecognitionService.Models
{
    public class MarkerConfig
    {
        public List<RegistredTangibleMarker> registredTangibles { get; private set; }

        public MarkerConfig()
        {
            this.registredTangibles = new List<RegistredTangibleMarker>();
        }

        public MarkerConfig(List<RegistredTangibleMarker> registredTangibles)
        {
            this.registredTangibles = registredTangibles;
        }

        public void Add(RegistredTangibleMarker tangible)
        {
            registredTangibles.Add(tangible);
        }

        public void Update(RegistredTangibleMarker tangible)
        {
            var registredTangibleIndex = registredTangibles.FindIndex(marker => marker.Id == tangible.Id);
            registredTangibles[registredTangibleIndex] = tangible;
        }
        public void Remove(RegistredTangibleMarker tangible)
        {
            registredTangibles.Remove(tangible);
        }

        public RegistredTangibleMarker GetTangibleWithId(int id)
        {
            var tangible = registredTangibles.Find(marker => marker.Id == id);
            return tangible;
        }

        public bool IsRegistredWithId(int id)
        {
            return registredTangibles.Any(registredTangible => registredTangible.Id == id);
        }

        public string Serialize()
        {
            var list = new JArray(registredTangibles.Select(tangible => tangible.Serialize()));
            return JsonConvert.SerializeObject(list);
        }

        public static MarkerConfig Deserialize(string json)
        {
            var tangibles = JToken.Parse(json).Value<JArray>();
            var registredTangibles = tangibles.Select(token => RegistredTangibleMarker.Deserialize((string)token)).ToList();
            return new MarkerConfig(registredTangibles);
        }
    }
}
