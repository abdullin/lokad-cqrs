using Microsoft.WindowsAzure.StorageClient;

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
    }
}
