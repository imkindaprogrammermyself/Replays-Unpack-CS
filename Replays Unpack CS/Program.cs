using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BlowFishCS;
using System.IO.Compression;

namespace Replays_Unpack_CS
{
    class NetPacket
    {
        public uint size;
        public string type;
        public float time;
        public MemoryStream rawData;

        public NetPacket(MemoryStream stream)
        {
            var payloadSize = new byte[4];
            var payloadType = new byte[4];
            var payloadTime = new byte[4];

            stream.Read(payloadSize);
            stream.Read(payloadType);
            stream.Read(payloadTime);

            size = BitConverter.ToUInt32(payloadSize);
            type = BitConverter.ToUInt32(payloadType).ToString("X2");
            time = BitConverter.ToSingle(payloadTime);

            var data = new byte[size];
            stream.Read(data);
            rawData = new MemoryStream(data);
        }
    }

    class BinaryStream
    {
        private uint length;
        public MemoryStream value;

        public BinaryStream(MemoryStream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var bLen = new byte[4];
            stream.Read(bLen);
            length = BitConverter.ToUInt32(bLen);
            var bValue = new byte[length];
            stream.Read(bValue);            
            value = new MemoryStream(bValue);
        }
    }

    class EntityMethod
    {
        public uint entityId;
        public uint messageId;
        public BinaryStream data;

        public EntityMethod(MemoryStream stream)
        {
            var bEntityId = new byte[4];
            var bMessageId = new byte[4];

            stream.Read(bEntityId);
            stream.Read(bMessageId);

            entityId = BitConverter.ToUInt32(bEntityId);
            messageId = BitConverter.ToUInt32(bMessageId);

            data = new BinaryStream(stream);
        }
    }


    class Program
    {
        static IEnumerable<(int, byte[])> ChunkData(byte[] data, int len = 8)
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

                idx += 1;
                yield return (idx, g);
            }
        }

        static void Main(string[] args)
        {
            using (FileStream fs = File.OpenRead(@"C:\Projects\Python\replay-data-extract\sample_replays\20210622_224157_PJSD219-Kitakaze_19_OC_prey.wowsreplay"))
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
                //Console.WriteLine(sReplayJSONData);

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
                            var decrypted_block = BitConverter.ToInt64(bfish.Decrypt_ECB(chunk.Item2));
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
                    compressedData.Seek(2, SeekOrigin.Begin); //DeflateStream doesn't strip the header so we strip it manually.
                    var decompressedData = new MemoryStream(); 
                    using (DeflateStream df = new(compressedData, CompressionMode.Decompress))
                    {
                        df.CopyTo(decompressedData);
                    }
                    //Console.WriteLine(decompressedData.Length);
                    decompressedData.Seek(0, SeekOrigin.Begin);
                    int called = 0;
                    while (decompressedData.Position != decompressedData.Length)
                    {
                        var np = new NetPacket(decompressedData);
                        //Console.WriteLine("{0}: {1}", np.time, np.type);
                        if (np.type == "08")
                        {
                            var em = new EntityMethod(np.rawData);
                            if (em.messageId == 115) // This will probably change.
                            {
                                Console.WriteLine("{0}: {1}", em.entityId, em.messageId);

                                var unk1 = new byte[8]; //?
                                em.data.value.Read(unk1);

                                var arenaID = new byte[8];
                                em.data.value.Read(arenaID);

                                var teamBuildTypeID = new byte[1];
                                em.data.value.Read(teamBuildTypeID);

                                var blobPreBattlesInfoSize = new byte[1];
                                em.data.value.Read(blobPreBattlesInfoSize);
                                var blobPreBattlesInfo = new byte[blobPreBattlesInfoSize[0]];
                                em.data.value.Read(blobPreBattlesInfo);

                                var blobPlayersStatesSize = new byte[1];
                                em.data.value.Read(blobPlayersStatesSize);

                                if (blobPlayersStatesSize[0] == 255)
                                {
                                    var blobPlayerStatesRealSize = new byte[2];
                                    em.data.value.Read(blobPlayerStatesRealSize);
                                    var PlayerStatesRealSize = BitConverter.ToUInt16(blobPlayerStatesRealSize);
                                    em.data.value.Read(new byte[1]); //?

                                    // blobPlayerStates will contain players' information like account id, server realm, etc...
                                    // but it is serialized via Python's pickle and there's no deserializer for that in C#.
                                    var blobPlayerStates = new byte[PlayerStatesRealSize];
                                    em.data.value.Read(blobPlayerStates);
                                    
                                }
                            }
                        }
                    }
                }
                Console.ReadLine();
            }
        }
    }
}
