using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.EntityFrameworkCore;

namespace Looksy.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly LooksyDbContext _context;
        private readonly string _containerName;

        public BlobService(IConfiguration config, BlobServiceClient blobServiceClient, LooksyDbContext context)
        {
            _blobServiceClient = blobServiceClient;
            _containerName = config["AzureBlob:ContainerName"];
            _context = context;
        }

        public async Task<string> UploadAsync(Stream stream, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        public string GenerateSasUrl(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        public string? GenerateSasUrlForLatestPhoto(int groupId)
        {
            // Najdi fotku s největším ID v dané skupině
            var photo = _context.Photos
                .Where(p => p.GroupId == groupId)
                .OrderByDescending(p => p.Id)
                .FirstOrDefault();

            if (photo == null)
                return null;

            // Extrahuj fileName z URL (např. "1/4.jpg")
            var uri = new Uri(photo.Url);
            var fileName = uri.AbsolutePath.TrimStart('/').Split("photos/").Last();

            // SAS URL
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
    }
}

