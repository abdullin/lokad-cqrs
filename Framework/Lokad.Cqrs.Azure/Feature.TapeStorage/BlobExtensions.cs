using System;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public static class BlobExtensions
    {
        public static bool Exists(this CloudBlob blob)
        {
            try
            {
                blob.FetchAttributes();
                return true;
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
                    return false;

                throw;
            }
        }

        public static void SetLength(this CloudPageBlob blob, long newLength, int timeout = 10000)
        {
            var credentials = blob.ServiceClient.Credentials;

            var requestUri = blob.Uri;
            if (credentials.NeedsTransformUri)
                requestUri = new Uri(credentials.TransformUri(requestUri.ToString()));

            var request = BlobRequest.SetProperties(requestUri, timeout, blob.Properties, null, newLength);
            request.Timeout = timeout;

            credentials.SignRequest(request);

            using (request.GetResponse()) {}
        }

        public static Stream OpenAppend(this CloudPageBlob blob)
        {
            return new PageBlobAppendStream(blob);
        }

        public static Stream OpenReadAppending(this CloudPageBlob blob)
        {
            return new PageBlobReadStream(blob);
        }
    }
}
