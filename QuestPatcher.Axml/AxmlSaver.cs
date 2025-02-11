﻿using System.IO;

namespace QuestPatcher.Axml
{
    public static class AxmlSaver
    {
        public static void SaveDocument(Stream stream, AxmlElement rootElement)
        {
            BinaryWriter mainOutput = new(stream);

            // Write the main elements chunk of the file to a MemoryStream first
            SavingContext ctx = new();
            rootElement.PrepareResourceIndices(ctx);
            rootElement.Save(ctx);
            MemoryStream mainChunkStream = (MemoryStream) ctx.Writer.BaseStream;


            string[] stringPool = ctx.StringPool.Save();
            int[] resourcePool = ctx.ResourcePool.Save();
            
            int stringPoolLength = StringPool.CalculatePoolLength(stringPool);
            int stringPoolPadding = 4 - stringPoolLength % 4;
            stringPoolLength += stringPoolPadding; // Add padding to four bytes
            
            int resourcePoolLength = resourcePool.Length * 4; // Each pool item is an integer

            // The length of the main xml tag is that of the whole file, so also including the string pool, resource pool, and main chunk. (+ extra 8 + 8 = 16 bytes for string pool and resource pool header)
            mainOutput.WriteChunkHeader(ResourceType.Xml, stringPoolLength + resourcePoolLength + (int) mainChunkStream.Position + 16);
            
            mainOutput.WriteChunkHeader(ResourceType.StringPool, stringPoolLength);
            StringPool.SaveStringPool(stringPool, mainOutput);
            for (int i = 0; i < stringPoolPadding; i++)
            {
                mainOutput.Write((byte) 0);
            }
            
            mainOutput.WriteChunkHeader(ResourceType.XmlResourceMap, resourcePoolLength);
            foreach (int resource in resourcePool)
            {
                mainOutput.Write(resource);
            }
            
            // Save the main chunk of the file
            mainChunkStream.Position = 0;
            mainChunkStream.CopyTo(stream);
        }
    }
}