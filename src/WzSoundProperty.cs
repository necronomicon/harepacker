﻿using System.IO;
using System;
using System.Text;

public class WzSoundProperty : WzExtended {
  #region Fields
  internal string name;
  internal byte[] mp3bytes = null;
  internal WzObject parent;
      internal int len_ms;
      internal byte[] header;
  //internal WzImage imgParent;
      internal WzBinaryReader wzReader;
      internal long offs;
      public static readonly int header_len = 82;
      public static readonly byte[] soundHeader = new byte[] { 0x02, 0x83, 0xEB, 0x36, 0xE4, 0x4F, 0x52, 0xCE, 0x11, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70, 0x8B, 0xEB, 0x36, 0xE4, 0x4F, 0x52, 0xCE, 0x11, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70, 0x00, 0x01, 0x81, 0x9F, 0x58, 0x05, 0x56, 0xC3, 0xCE, 0x11, 0xBF, 0x01, 0x00, 0xAA, 0x00, 0x55, 0x59 };
      internal int unk0 = 0x551E5A;
      internal short channels;
      internal int frequency;
      internal int byterate;
      internal short unk1 = 1;
      internal short unk2 = 0;
      internal short extraSize;
      internal short unk4 = 1;
      internal short unk5 = 2;
      internal short unk6 = 0;
      internal short blockSize;
      internal short unk8 = 1;
      internal short unk9 = 0; // can be large numbers

  #endregion

  #region Inherited Members

      public override WzImageProperty DeepClone()
      {
          WzSoundProperty clone = new WzSoundProperty(name, len_ms, header, mp3bytes);
          return clone;
      }

  public override object WzValue { get { return GetBytes(false); } }

      public override void SetValue(object value)
      {
          return;
      }
  /// <summary>
  /// The parent of the object
  /// </summary>
  public override WzObject Parent { get { return parent; } internal set { parent = value; } }
  /*/// <summary>
  /// The image that this property is contained in
  /// </summary>
  public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/
  /// <summary>
  /// The name of the property
  /// </summary>
  public override string Name { get { return name; } set { name = value; } }
  /// <summary>
  /// The WzPropertyType of the property
  /// </summary>
  public override WzPropertyType PropertyType { get { return WzPropertyType.Sound; } }
  public override void WriteValue(WzBinaryWriter writer)
  {
          byte[] data = GetBytes(false);
    writer.WriteStringValue("Sound_DX8", 0x73, 0x1B);
    writer.Write((byte)0);
    writer.WriteCompressedInt(data.Length);
    writer.WriteCompressedInt(len_ms);
          writer.Write(header);
    writer.Write(data);
  }
  public override void ExportXml(StreamWriter writer, int level)
  {
    writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.EmptyNamedTag("WzSound", this.Name));
  }
  /// <summary>
  /// Disposes the object
  /// </summary>
  public override void Dispose()
  {
    name = null;
    mp3bytes = null;
  }
  #endregion

  #region Custom Members
  /// <summary>
  /// The data of the mp3 header
  /// </summary>
      public byte[] Header { get { return header; } set { header = value; } }
      /// <summary>
      /// Length of the mp3 file in milliseconds
      /// </summary>
      public int Length { get { return len_ms; } set { len_ms = value; } }
      /// <summary>
      /// Frequency of the mp3 file in Hz
      /// </summary>
      public int Frequency { get { return frequency; } set { frequency = value; } }
      /// <summary>
      /// BPS of the mp3 file
      /// </summary>
      //public byte BPS { get { return bps; } set { bps = value; } }
  /// <summary>
  /// Creates a WzSoundProperty with the specified name
  /// </summary>
  /// <param name="name">The name of the property</param>
      /// <param name="reader">The wz reader</param>
      /// <param name="parseNow">Indicating whether to parse the property now</param>
      public WzSoundProperty(string name, WzBinaryReader reader, bool parseNow)
  {
    this.name = name;
          wzReader = reader;
          reader.BaseStream.Position++;
          offs = reader.BaseStream.Position;
          //note - soundDataLen does NOT include the length of the header.
          int soundDataLen = reader.ReadCompressedInt();
          len_ms = reader.ReadCompressedInt();
          header = reader.ReadBytes(header_len);
          ParseHeader();
          if (parseNow)
              mp3bytes = reader.ReadBytes(soundDataLen);
          else
              reader.BaseStream.Position += soundDataLen;
  }

      /*public WzSoundProperty(string name)
      {
          this.name = name;
          this.len_ms = 0;
          this.header = null;
          this.mp3bytes = null;
      }*/

      /// <summary>
      /// Creates a WzSoundProperty with the specified name and data
      /// </summary>
      /// <param name="name">The name of the property</param>
      /// <param name="len_ms">The sound length</param>
      /// <param name="header">The sound header</param>
      /// <param name="data">The sound data</param>
      public WzSoundProperty(string name, int len_ms, byte[] header, byte[] data)
      {
          this.name = name;
          this.len_ms = len_ms;
          this.header = header;
          this.mp3bytes = data;
          ParseHeader();
      }
      /// <summary>
      /// Creates a WzSoundProperty with the specified name from a file
      /// </summary>
      /// <param name="name">The name of the property</param>
      /// <param name="file">The path to the sound file</param>
      public WzSoundProperty(string name, string file)
      {
          this.name = name;
          Mp3FileReader reader = new Mp3FileReader(file);
          this.len_ms = (int)((double)reader.Length * 1000d / (double)reader.WaveFormat.AverageBytesPerSecond);
          this.channels = (short)reader.Mp3WaveFormat.Channels;
          this.frequency = reader.Mp3WaveFormat.SampleRate;
          this.byterate = reader.Mp3WaveFormat.AverageBytesPerSecond;
          this.extraSize = (short)reader.Mp3WaveFormat.ExtraSize;
          this.blockSize = (short)reader.Mp3WaveFormat.blockSize;
          RebuildHeader();
          reader.Dispose();
          this.mp3bytes = File.ReadAllBytes(file);
      }

      public static bool memcmp(byte[] a, byte[] b, int n)
      {
          for (int i = 0; i < n; i++)
          {
              if (a[i] != b[i])
              {
                  return false;
              }
          }
          return true;
      }

      public static string ByteArrayToString(byte[] ba)
      {
          StringBuilder hex = new StringBuilder(ba.Length * 3);
          foreach (byte b in ba)
              hex.AppendFormat("{0:x2} ", b);
          return hex.ToString();
      }

      public void RebuildHeader()
      {
          using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
          {
              bw.Write(soundHeader);
              bw.Write(unk0);
              bw.Write(channels);
              bw.Write(frequency);
              bw.Write(byterate);
              bw.Write(unk1);
              bw.Write(unk2);
              bw.Write(extraSize);
              bw.Write(unk4);
              bw.Write(unk5);
              bw.Write(unk6);
              bw.Write(blockSize);
              bw.Write(unk8);
              bw.Write(unk9);
              header = ((MemoryStream)bw.BaseStream).ToArray();
          }
      }

      private void LogSoundHeader()
      {
          Console.WriteLine("Weird sound header: " + ByteArrayToString(header));
      }

      private void ParseHeader()
      {
          using (BinaryReader br = new BinaryReader(new MemoryStream(header)))
          {
              br.ReadBytes(soundHeader.Length);
              unk0 = br.ReadInt32();
              channels = br.ReadInt16();
              frequency = br.ReadInt32();
              byterate = br.ReadInt32();
              unk1 = br.ReadInt16();
              unk2 = br.ReadInt16();
              extraSize = br.ReadInt16();
              unk4 = br.ReadInt16();
              unk5 = br.ReadInt16();
              unk6 = br.ReadInt16();
              blockSize = br.ReadInt16();
              unk8 = br.ReadInt16();
              unk9 = br.ReadInt16();
          }
      }
      #endregion

      #region Parsing Methods
      public byte[] GetBytes(bool saveInMemory)
      {
          if (mp3bytes != null)
              return mp3bytes;
          else
          {
              if (wzReader == null) return null;
              long currentPos = wzReader.BaseStream.Position;
              wzReader.BaseStream.Position = offs;
              int soundDataLen = wzReader.ReadCompressedInt();
              wzReader.ReadCompressedInt();
              wzReader.BaseStream.Position += header_len;
              mp3bytes = wzReader.ReadBytes(soundDataLen);
              wzReader.BaseStream.Position = currentPos;
              if (saveInMemory)
                  return mp3bytes;
              else
              {
                  byte[] result = mp3bytes;
                  mp3bytes = null;
                  return result;
              }
          }
      }

      public void SaveToFile(string file)
      {
          File.WriteAllBytes(file, GetBytes(false));
      }
  #endregion

      #region Cast Values
      public override byte[] GetBytes()
      {
          return GetBytes(false);
      }
      #endregion
}
