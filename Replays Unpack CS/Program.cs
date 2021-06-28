using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BlowFishCS;
using System.IO.Compression;

namespace Replays_Unpack_CS
{
    class Chunked
    {
        public int index;
        public byte[] chunk;
    }

    class Program
    {
        static IEnumerable<Chunked> ChunkData(byte[] data, int len = 8)
        {
            int idx = 0;
            for (var s = 0; s <= data.Length; s += len)
            {
                byte[] g;
                try
                {
                    g = data[s..(s + len)];
                }
                catch (ArgumentOutOfRangeException)
                {
                    g = data[s..];
                }

                var c = new Chunked()
                {
                    index = idx,
                    chunk = g
                };
                idx += 1;
                yield return c;
            }
        }

        static void Main(string[] args)
        {
            using (FileStream fs = File.OpenRead(@"C:\Games\World_of_Warships\replays\20210627_151609_PBSD598-Black-Cossack_53_Shoreside.wowsreplay"))
            {
                byte[] bReplaySignature = new byte[4];
                byte[] bReplayBlockCount = new byte[4];
                byte[] bReplayBlockSize = new byte[4];
                byte[] bReplayJSONData;

                fs.Read(bReplaySignature, 0, 4);
                fs.Read(bReplayBlockCount, 0, 4);
                fs.Read(bReplayBlockSize, 0, 4);

                int jsonDataSize = BitConverter.ToInt32(bReplayBlockSize, 0);

                bReplayJSONData = new byte[jsonDataSize];
                fs.Read(bReplayJSONData, 0, jsonDataSize);

                string sReplayJSONData = Encoding.UTF8.GetString(bReplayJSONData);
                Console.WriteLine(sReplayJSONData);

                using (var memStream = new MemoryStream())
                {
                    fs.CopyTo(memStream);
                    var sBfishKey = "\x29\xB7\xC9\x09\x38\x3F\x84\x88\xFA\x98\xEC\x4E\x13\x19\x79\xFB";
                    var bBfishKey = sBfishKey.Select(x => Convert.ToByte(x)).ToArray();
                    var bfish = new BlowFish(bBfishKey);
                    long prev = 0;
                    using var compressedData = new MemoryStream();
                    foreach (var chunk in ChunkData(memStream.ToArray()[8..]))
                    {
                        try
                        {
                            var decrypted_block = BitConverter.ToInt64(bfish.Decrypt_ECB(chunk.chunk));
                            if (prev != 0)
                            {
                                decrypted_block ^= prev;
                            }
                            prev = decrypted_block;
                            compressedData.Write(BitConverter.GetBytes(decrypted_block));
                        }
                        catch (ArgumentOutOfRangeException)
                        {

                        }
                    }
                    //78, DA. VALID ZLIB HEADER.
                    compressedData.Seek(2, SeekOrigin.Begin); //SKIP THE HEADER
                    var decompressedData = new MemoryStream(); 
                    using (DeflateStream df = new(compressedData, CompressionMode.Decompress))
                    {
                        df.CopyTo(decompressedData);
                    }
                    Console.WriteLine(decompressedData.Length);
                    decompressedData.Seek(0, SeekOrigin.Begin);
                    while (decompressedData.Position != decompressedData.Length)
                    {
                        var payloadSize = new byte[4];
                        var payloadType = new byte[4];
                        var payloadTime = new byte[4];

                        decompressedData.Read(payloadSize);
                        decompressedData.Read(payloadType);
                        decompressedData.Read(payloadTime);

                        var size = BitConverter.ToUInt32(payloadSize);
                        var type = BitConverter.ToUInt32(payloadType);
                        var time = BitConverter.ToSingle(payloadTime);

                        var payload = new byte[size];
                        decompressedData.Read(payload);

                        Console.WriteLine("{0}: {1}: {2}", decompressedData.Position, time, type.ToString("X2"));
                    }
                }
                Console.ReadLine();
            }
        }
    }
}
