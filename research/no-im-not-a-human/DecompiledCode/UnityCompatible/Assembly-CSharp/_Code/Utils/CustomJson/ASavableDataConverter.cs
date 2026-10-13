using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Utils.CustomJson
{
	public sealed class ASavableDataConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(ASavableData);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			string text = (string?)jObject["$type"];
			if (string.IsNullOrEmpty(text))
			{
				throw new JsonSerializationException($"Missing or empty $type in JSON for objectType '{objectType}'");
			}
			Type type = Type.GetType(text);
			if (type == null)
			{
				throw new JsonSerializationException("Unable to resolve type '" + text + "'");
			}
			return jObject.ToObject(type, serializer);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject.FromObject(value, serializer).WriteTo(writer);
		}
	}
}
