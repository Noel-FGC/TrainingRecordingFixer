using System;
using System.IO;

namespace TrainingRecordingFixer {
  class Program {
    static void Main(string[] args) {
      if (args.Length == 0) {
        Console.WriteLine("No Arguments Passed, Please Pass Atleast One Argument");
        Console.WriteLine("Usage: ");
        Console.WriteLine("    TrainingRecordingFixer.exe Input.bin [Output.bin]");
        Console.ReadKey();
        Environment.Exit(1);
      }
      string output = Path.ChangeExtension(args[0], "").TrimEnd('.') + "_OUT.bin";
      if (args.Length == 2) {
        output = args[1];
      }
      var readStream = File.Open(args[0], FileMode.Open);
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
    }
  }
}
