using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MiniGame.Core
{
    public sealed class SnapshotValidationException : Exception
    {
        public SnapshotValidationException(string message) : base(message) { }
        public SnapshotValidationException(string message, Exception inner) : base(message, inner) { }
    }

    public static class SnapshotCodec
    {
        public const int CurrentSchemaVersion = 1;

        private static JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture,
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Error,
            MaxDepth = 64,
            Converters = { new StringEnumConverter { AllowIntegerValues = false } }
        };

        public static string Serialize(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            return Encode(session.CreateSnapshot(), Formatting.Indented);
        }

        public static GameSession Deserialize(string json, GameRules rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (string.IsNullOrWhiteSpace(json)) throw new SnapshotValidationException("Snapshot JSON is empty.");
            try
            {
                var snapshot = Decode<GameSnapshot>(json);
                if (snapshot == null) throw new SnapshotValidationException("Snapshot must be an object.");
                return GameSession.Restore(snapshot, rules);
            }
            catch (JsonException exception)
            {
                throw new SnapshotValidationException("Snapshot JSON is malformed or has an unsupported shape.", exception);
            }
        }

        internal static string Canonical(object value) => Encode(value, Formatting.None);

        internal static T Copy<T>(T value) where T : class
        {
            return Decode<T>(Canonical(value))
                ?? throw new SnapshotValidationException("Snapshot copy failed.");
        }

        private static string Encode(object value, Formatting formatting)
        {
            // Create, rather than CreateDefault, isolates us from the host's JsonConvert.DefaultSettings.
            var serializer = JsonSerializer.Create(Settings);
            using var buffer = new StringWriter(CultureInfo.InvariantCulture);
            using var writer = new JsonTextWriter(buffer) { Formatting = formatting };
            serializer.Serialize(writer, value);
            writer.Flush();
            return buffer.ToString();
        }

        private static T? Decode<T>(string json)
        {
            using var buffer = new StringReader(json);
            using var reader = new JsonTextReader(buffer) { MaxDepth = 64, DateParseHandling = DateParseHandling.None };
            var result = JsonSerializer.Create(Settings).Deserialize<T>(reader);
            if (reader.Read()) throw new JsonSerializationException("Snapshot has additional JSON content.");
            return result;
        }
    }

    public interface ISaveStore
    {
        void Save(GameSession session);
        GameSession Load(GameRules rules);
    }

    // The host supplies the path. Core does not use Unity's Application or a global save directory.
    public sealed class FileSaveStore : ISaveStore
    {
        public string FilePath { get; }
        public FileSaveStore(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("A save path is required.", nameof(filePath));
            FilePath = Path.GetFullPath(filePath);
        }

        public void Save(GameSession session)
        {
            var json = SnapshotCodec.Serialize(session);
            var directory = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(directory);
            var temporary = Path.Combine(directory, $".{Path.GetFileName(FilePath)}.{Guid.NewGuid():N}.tmp");
            try
            {
                var bytes = new UTF8Encoding(false).GetBytes(json);
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(FilePath)) File.Replace(temporary, FilePath, null);
                else File.Move(temporary, FilePath);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        public GameSession Load(GameRules rules) => SnapshotCodec.Deserialize(File.ReadAllText(FilePath, Encoding.UTF8), rules);
    }
}
