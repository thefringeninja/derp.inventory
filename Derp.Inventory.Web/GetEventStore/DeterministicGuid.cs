using System;
using System.Security.Cryptography;

namespace Derp.Inventory.Web.GetEventStore
{
    /// <summary>
    ///     Helper methods for working with <see cref="Guid" />.
    /// </summary>
    public static class DeterministicGuid
    {
        private static readonly Guid @namespace = Guid.Parse("EEFB6C8D-29BC-4EDE-9CED-F8621C76F1F8");

        public static Guid CreateFrom(Guid commitId, int aggregateVersion)
        {
            var data = new byte[20];
            commitId.ToByteArray().CopyTo(data, 0);
            BitConverter.GetBytes(aggregateVersion).CopyTo(data, 16);
            return CreateFrom(@namespace, 5, data);
        }

        private static Guid CreateFrom(Guid namespaceId, int version, byte[] nameBytes)
        {
            if (version != 3 && version != 5)
                throw new ArgumentOutOfRangeException("version", "version must be either 3 or 5.");

            if (nameBytes == null)
                throw new ArgumentNullException("nameBytes");

            // convert the namespace UUID to network order (step 3)
            var namespaceBytes = namespaceId.ToByteArray();
            SwapByteOrder(namespaceBytes);

            // comput the hash of the name space ID concatenated with the name (step 4)
            byte[] hash;
            using (var algorithm = version == 3 ? (HashAlgorithm) MD5.Create() : SHA1.Create())
            {
                algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
                algorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
                hash = algorithm.Hash;
            }

            // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
            var newGuid = new byte[16];
            Array.Copy(hash, 0, newGuid, 0, 16);

            // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
            newGuid[6] = (byte) ((newGuid[6] & 0x0F) | (version << 4));

            // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
            newGuid[8] = (byte) ((newGuid[8] & 0x3F) | 0x80);

            // convert the resulting UUID to local byte order (step 13)
            SwapByteOrder(newGuid);
            return new Guid(newGuid);
        }

        // Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
        private static void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }

        private static void SwapBytes(byte[] guid, int left, int right)
        {
            var temp = guid[left];
            guid[left] = guid[right];
            guid[right] = temp;
        }
    }
}