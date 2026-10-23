#nullable enable

using System;
using System.IO;
using System.Text;
using MiniGame.Core;

namespace MiniGame.Presentation
{
    public sealed class RuntimeFileSaveStore
    {
        public string FilePath { get; }
        public RuntimeFileSaveStore(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("A save path is required.", nameof(filePath));
            FilePath = Path.GetFullPath(filePath);
        }

        public void Save(GameRuntime runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            SaveSerialized(runtime.Save());
        }

        public void SaveSerialized(string checkpoint)
        {
            if (string.IsNullOrWhiteSpace(checkpoint)) throw new ArgumentException("A checkpoint is required.", nameof(checkpoint));
            var bytes = new UTF8Encoding(false).GetBytes(checkpoint);
            var directory = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(directory);
            var temporary = Path.Combine(directory, $".{Path.GetFileName(FilePath)}.{Guid.NewGuid():N}.tmp");
            try
            {
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

        public GameRuntime Load(GameRules rules, IGameContent content)
            => GameRuntime.Restore(File.ReadAllText(FilePath, Encoding.UTF8), rules, content);
    }
}
