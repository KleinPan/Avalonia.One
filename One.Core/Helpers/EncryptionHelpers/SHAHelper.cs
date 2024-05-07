using Org.BouncyCastle.Crypto.Digests;

using System;

namespace One.Base.Helpers.EncryptionHelpers
{
    public static class SHAHelper
    {
      public  static string CalculateFileSHA1(string filePath)
        {
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                var sha1 = new Sha1Digest();
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    sha1.BlockUpdate(buffer, 0, bytesRead);
                }

                byte[] hash = new byte[sha1.GetDigestSize()];
                sha1.DoFinal(hash, 0);

                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public static string CalculateFileSHA256(string filePath)
        {
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                var sha256 = new Sha256Digest();
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    sha256.BlockUpdate(buffer, 0, bytesRead);
                }

                byte[] hash = new byte[sha256.GetDigestSize()];
                sha256.DoFinal(hash, 0);

                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
