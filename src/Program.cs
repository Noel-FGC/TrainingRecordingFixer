using System;
using System.IO;
using System.CommandLine;

namespace TrainingRecordingFixer {
  class Program {
    static void Main(string[] args) {
      var rootCommand = new RootCommand();
      var pathArg = new Argument<string>(name: "inpath", description: "path to bbsave.dat or bin to convert into challenge sample data");
      var outArg = new Argument<string>(name: "outpath", description: "path to save outputted file to") { Arity = ArgumentArity.ZeroOrOne };
      rootCommand.Add(pathArg);
      rootCommand.Add(outArg);
      rootCommand.SetHandler((path, output) => {
        output ??= $"{Path.ChangeExtension(path, "").TrimEnd('.')}_OUT.bin";
        if (path.EndsWith(".dat")) {
          handleDat(path, output);
        } else {
          handleBin(path, output);
        }
      }, pathArg, outArg);
      rootCommand.Invoke(args);
    }

    static void handleDat(string path, string output) {
      var readStream = File.Open(path, FileMode.Open);
      var reader = new BinaryReader(readStream);
      var writeStream = File.Open(output, FileMode.Create);

      readStream.Seek(0xf008, SeekOrigin.Begin); // slot 1 frame length
      int length = reader.ReadInt32();
      int endPos = 0xf018 + length * 2; // 0xf018 is where the actual recording starts, each frame is 2 bytes

      readStream.Seek(12, SeekOrigin.Current); // jump forward to 0xf018

      while (readStream.Position < endPos) {
        var frame = reader.ReadBytes(2);
        writeStream.Write(frame);
        writeStream.Write((byte[])[0x00, 0x00]);
      }
      writeStream.Flush();
    }

    static void handleBin(string path, string output) {
      var readStream = File.Open(path, FileMode.Open);
      var reader = new BinaryReader(readStream);
      var writeStream = File.Open(output, FileMode.Create);

      while (readStream.Position < readStream.Length) {
        var frame = reader.ReadBytes(2);
        if (frame[0] == 0x00) {
          break;
        }
        writeStream.Write(frame);
        writeStream.Write((byte[])[0x00, 0x00]);
      }
      writeStream.Flush();
    }
  }
}
