using System.Security.Cryptography;

namespace Communication.Common
{
    public class RsaPublicKeyParameters
    {
        public byte[] D;
        public byte[] DP;
        public byte[] DQ;
        public byte[] Exponent;

        public byte[] InverseQ;
        public byte[] Modulus;
        public byte[] P;
        public byte[] Q;

        public RsaPublicKeyParameters(RSAParameters info)
        {
            D = info.D;
            DP = info.DP;
            DQ = info.DQ;
            Exponent = info.Exponent;
            InverseQ = info.InverseQ;
            Modulus = info.Modulus;
            P = info.P;
            Q = info.Q;
        }

        public RSAParameters GetRSAParameters()
        {
            return new RSAParameters
            {
                D = D,
                DP = DP,
                DQ = DQ,
                Exponent = Exponent,
                InverseQ = InverseQ,
                Modulus = Modulus,
                P = P,
                Q = Q
            };
        }
    }
}
