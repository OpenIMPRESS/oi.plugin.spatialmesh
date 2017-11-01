// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using SysDiag = System.Diagnostics;
using System.IO;
using UnityEngine;

namespace oi.plugin.spatialmesh {

    public static class IdsSerializer {
        /// <summary>
        /// The mesh header consists of two 32 bit integers.
        /// </summary>
        private static int HeaderSize = sizeof(int) * 2;


        public static byte[] Serialize(List<int> ids) {
            byte[] data = null;

            using (MemoryStream stream = new MemoryStream()) {
                using (BinaryWriter writer = new BinaryWriter(stream)) {
                    WriteIdList(writer, ids);

                    stream.Position = 0;
                    data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                }
            }

            return data;
        }


        public static IEnumerable<int> Deserialize(byte[] data) {
            List<int> ids = new List<int>();

            using (MemoryStream stream = new MemoryStream(data)) {
                using (BinaryReader reader = new BinaryReader(stream)) {
                    while (reader.BaseStream.Length - reader.BaseStream.Position >= HeaderSize) {
                        ids.Add(reader.ReadInt32());
                    }
                }
            }

            return ids;
        }


        private static void WriteIdList(BinaryWriter writer, List<int> ids) {
            SysDiag.Debug.Assert(writer != null);

            // Write the mesh data.
            WriteIdsHeader(writer, ids.Count);
            WriteIds(writer, ids);
        }

        private static void WriteIdsHeader(BinaryWriter writer, int idsCount) {
            SysDiag.Debug.Assert(writer != null);

            writer.Write(0); // '0' announces ids list packet
            writer.Write(idsCount);
        }

        private static void WriteIds(BinaryWriter writer, List<int> ids) {
            SysDiag.Debug.Assert(writer != null);

            foreach (int id in ids) {
                writer.Write(id);
            }
        }

    }
}