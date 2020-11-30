using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Communication.Common
{
    public class DesParameters
    {
        public byte[] IV;
        public byte[] Key;

        public DesParameters(byte[] IV, byte[] Key)
        {
            this.IV = IV;
            this.Key = Key;
        }
    }
}
