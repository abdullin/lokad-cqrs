using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Lokad.Cqrs.Xmpp
{
	public delegate bool XmppCertificateValidationCallback(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors);
}