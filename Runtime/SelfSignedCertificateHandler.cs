using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Networking;

namespace ArenaUnity
{
    public class SelfSignedCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            X509Certificate2 certificate = new X509Certificate2(certificateData);
            string host = certificate.GetNameInfo(X509NameType.SimpleName, false);
            Debug.LogWarning($"Excepting server certificate without verification for HTTP on {host}!");
            return true;
        }
    }
}
