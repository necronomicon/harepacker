﻿using System;
using System.IO;
using System.Security.Cryptography;

public class WzKeyGenerator
{
  #region Methods
  /// <summary>
  /// Generates the wz key used in the encryption from ZLZ.dll
  /// </summary>
  /// <param name="pathToZlz">Path to ZLZ.dll</param>
  /// <returns>The wz key</returns>
  public static byte[] GenerateKeyFromZlz(string pathToZlz)
  {
    FileStream zlzStream = File.OpenRead(pathToZlz);
    byte[] wzKey = GenerateWzKey(GetIvFromZlz(zlzStream), GetAesKeyFromZlz(zlzStream));
    zlzStream.Close();
    return wzKey;
  }

  public static byte[] GetIvFromZlz(FileStream zlzStream)
  {
    byte[] iv = new byte[4];

    zlzStream.Seek(0x10040, SeekOrigin.Begin);
    zlzStream.Read(iv, 0, 4);
    return iv;
  }

  private static byte[] GetAesKeyFromZlz(FileStream zlzStream)
  {
    byte[] aes = new byte[32];

    zlzStream.Seek(0x10060, SeekOrigin.Begin);
    for (int i = 0; i < 8; i++)
    {
      zlzStream.Read(aes, i * 4, 4);
      zlzStream.Seek(12, SeekOrigin.Current);
    }
    return aes;
  }

  public static byte[] GenerateWzKey(byte[] WzIv)
  {
    return GenerateWzKey(WzIv, CryptoConstants.getTrimmedUserKey());
  }

  public static byte[] GenerateWzKey(byte[] WzIv, byte[] AesKey)
  {
    if (BitConverter.ToInt32(WzIv, 0) == 0)
    {
      return new byte[ushort.MaxValue];
    }
    AesManaged crypto = new AesManaged();
    crypto.KeySize = 256;
    crypto.Key = AesKey;
    crypto.Mode = CipherMode.ECB;

    MemoryStream memStream = new MemoryStream();
    CryptoStream cryptoStream = new CryptoStream(memStream, crypto.CreateEncryptor(), CryptoStreamMode.Write);

    byte[] input = MapleCrypto.multiplyBytes(WzIv, 4, 4);
    byte[] wzKey = new byte[ushort.MaxValue];
    for (int i = 0; i < (wzKey.Length / 16); i++)
    {
      cryptoStream.Write(input, 0, 16);
      input = memStream.ToArray();
      Array.Copy(input, 0, wzKey, (i * 16), 16);
      memStream.Position = 0;
    }
          cryptoStream.Write(input, 0, 16);
          Array.Copy(memStream.ToArray(), 0, wzKey, (wzKey.Length - 15), 15);
    try
    {
      cryptoStream.Dispose();
      memStream.Dispose();
    }
    catch (Exception e)
    {
      Console.WriteLine("Error disposing AES streams" + e);
    }

    return wzKey;
  }
  #endregion
}
