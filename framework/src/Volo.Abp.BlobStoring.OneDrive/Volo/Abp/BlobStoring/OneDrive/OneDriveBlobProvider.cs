using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using static Azure.Core.HttpHeader;

namespace Volo.Abp.BlobStoring.OneDrive;

public class OneDriveBlobProvider : BlobProviderBase, ITransientDependency
{
    protected IOneDriveBlobNameCalculator OneDriveBlobNameCalculator { get; }
    protected IBlobNormalizeNamingService BlobNormalizeNamingService { get; }

    public OneDriveBlobProvider(
        IOneDriveBlobNameCalculator sharePointBlobNameCalculator,
        IBlobNormalizeNamingService blobNormalizeNamingService)
    {
        OneDriveBlobNameCalculator = sharePointBlobNameCalculator;
        BlobNormalizeNamingService = blobNormalizeNamingService;
    }

    public override async Task SaveAsync(BlobProviderSaveArgs args)
    {
        var blobName = OneDriveBlobNameCalculator.Calculate(args);
        var configuration = args.Configuration.GetOneDriveConfiguration();

        if (!args.OverrideExisting && await BlobExistsAsync(args))
        {
            throw new BlobAlreadyExistsException($"Saving BLOB '{args.BlobName}' does already exists in the container '{args.ContainerName}'! Set {nameof(args.OverrideExisting)} if it should be overwritten.");
        }

        if (configuration.CreateContainerIfNotExists)
        {
            await CreateContainerIfNotExists(args);
        }

        var client = GetGraphServiceClient(args);

        var result = await UploadDriveItem(args);
    }

    public override async Task<bool> DeleteAsync(BlobProviderDeleteArgs args)
    {
        var blobName = OneDriveBlobNameCalculator.Calculate(args);
        var configuration = args.Configuration.GetOneDriveConfiguration();

        var client = GetGraphServiceClient(args);
        var driveItem = await client.Drives[configuration.DriveId].Items[configuration.RootPathId].ItemWithPath(blobName).GetAsync();
        await client.Drives[configuration.DriveId].Items[configuration.RootPathId].DeleteAsync();

        return true;
    }

    public override async Task<bool> ExistsAsync(BlobProviderExistsArgs args)
    {
        var blobName = OneDriveBlobNameCalculator.Calculate(args);

        return await BlobExistsAsync(args);
    }

    public override async Task<Stream> GetOrNullAsync(BlobProviderGetArgs args)
    {
        var blobName = OneDriveBlobNameCalculator.Calculate(args);
        var configuration = args.Configuration.GetOneDriveConfiguration();

        if (string.IsNullOrEmpty(blobName)) return null;

        if (!await BlobExistsAsync(args))
        {
            return null;
        }

        var client = GetGraphServiceClient(args);
        var download = await client.Drives[configuration.DriveId].Items[configuration.RootPathId].ItemWithPath(blobName).Content.GetAsync();
        return await TryCopyToMemoryStreamAsync(download, args.CancellationToken);
    }

    protected virtual GraphServiceClient GetGraphServiceClient(BlobProviderArgs args)
    {
        var scopes = new[] { "https://graph.microsoft.com/.default" };

        var configuration = args.Configuration.GetOneDriveConfiguration();

        var options = new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };

        //var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, secret, options);
        var clientSecretCredential = new ClientSecretCredential(configuration.TenantId, configuration.ClientId, configuration.Secret, options);
        return new GraphServiceClient(clientSecretCredential, scopes);
    }

    protected virtual async Task CreateContainerIfNotExists(BlobProviderArgs args)
    {
        var configuration = args.Configuration.GetOneDriveConfiguration();
        var graphServiceClient = GetGraphServiceClient(args);

        string currentPath = string.Empty;
        string currentPathId = configuration.RootPathId;
        string relativePath = args.ContainerName;

        // Remove the root from the relative path.
        relativePath = relativePath.Replace(configuration.RootPath, string.Empty);
        if (relativePath.StartsWith("/")) relativePath = relativePath.Remove(0, 1);
        if (string.IsNullOrEmpty(relativePath)) return;

        // Test the full path to see if it alread exists.
        try
        {
            var currentPathItem = await graphServiceClient.Drives[configuration.DriveId].Items[configuration.RootPathId].ItemWithPath(relativePath).GetAsync();
            currentPathId = currentPathItem.Id;
        }
        catch { }

        if (currentPathId != configuration.RootPathId)
        {
            return;
        }

        // Loop through the path parts getting the folder or creating it

        foreach (string folderName in relativePath.Split('/'))
        {
            currentPath = JoinPaths(currentPath, folderName);

            try
            {
                var currentPathItem = await graphServiceClient.Drives[configuration.DriveId].Items[configuration.RootPathId].ItemWithPath(relativePath).GetAsync();
                currentPathId = currentPathItem.Id;
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (ex is ServiceException exception && exception.IsMatch(GraphErrorCode.ItemNotFound.ToString()))
                    {
                        DriveItem newFolder = new DriveItem()
                        {
                            Name = folderName,
                            Folder = new Folder(),
                        };

                        var currentPathItem = graphServiceClient.Drives[configuration.DriveId].Items[configuration.RootPathId].ItemWithPath(relativePath).GetAsync().Result;
                        currentPathId = currentPathItem.Id;

                        return true;
                    };

                    return false;
                });
            }
        }
    }

    protected virtual async Task<bool> BlobExistsAsync(BlobProviderArgs args)
    {
        var configuration = args.Configuration.GetOneDriveConfiguration();
        var client = GetGraphServiceClient(args);
        var blobName = OneDriveBlobNameCalculator.Calculate(args);

        // Test the full path to see if it alread exists.
        try
        {
            var currentPathItem = await client.Drives[configuration.DriveId].Items[configuration.RootPathId].ItemWithPath(blobName).GetAsync();
        }
        catch
        {
            return false;
        }

        return true;
    }

    protected virtual async Task<DriveItem> GetDriveItem(BlobProviderArgs args)
    {
        var configuration = args.Configuration.GetOneDriveConfiguration();
        var client = GetGraphServiceClient(args);
        var blobName = OneDriveBlobNameCalculator.Calculate(args);

        var driveItem = await client.Drives[configuration.DriveId].Items[configuration.RootPathId].ItemWithPath(blobName).GetAsync();
        return driveItem;
    }

    protected virtual async Task<UploadResult<DriveItem>> UploadDriveItem(BlobProviderSaveArgs args)
    {
        var configuration = args.Configuration.GetOneDriveConfiguration();
        var blobName = OneDriveBlobNameCalculator.Calculate(args);
        var client = GetGraphServiceClient(args);

        var uploadSessionRequestBody = new CreateUploadSessionPostRequestBody
        {
            Item = new DriveItemUploadableProperties
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "@microsoft.graph.conflictBehavior", "replace" }
                }
            }
        };

        UploadSession uploadSession = await client.Drives[configuration.DriveId].Items[configuration.RootPathId].ItemWithPath(blobName).CreateUploadSession.PostAsync(uploadSessionRequestBody);

        long fileSize = args.BlobStream.Length;
        int maxSliceSize = 10 * 320 * 1024;
        var fileUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, args.BlobStream, maxSliceSize);

        return await fileUploadTask.UploadAsync();
    }

    protected string JoinPaths(string part1, string part2)
    {
        string joined;

        if (string.IsNullOrEmpty(part1))
        {
            joined = part2;
        }
        else if (string.IsNullOrEmpty(part2))
        {
            joined = part1;
        }
        else
        {
            joined = string.Join("/", new string[] { part1, part2 });
        }

        return joined;
    }
}