using Microsoft.Graph.Models.ExternalConnectors;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.BlobStoring.OneDrive;

public class DefaultOneDriveBlobNameCalculator : IOneDriveBlobNameCalculator, ITransientDependency
{
    protected ICurrentTenant CurrentTenant { get; }

    public DefaultOneDriveBlobNameCalculator(ICurrentTenant currentTenant)
    {
        CurrentTenant = currentTenant;
    }

    public virtual string Calculate(BlobProviderArgs args)
    {
        var configuration = args.Configuration.GetOneDriveConfiguration();
        
        // Remove the root from the relative path.
        string blobName = args.BlobName;
        blobName = blobName.Replace(configuration.RootPath, string.Empty);
        if (blobName.StartsWith("/")) blobName = blobName.Remove(0, 1);
        return blobName;

        //return CurrentTenant.Id == null
        //    ? $"host/{args.BlobName}"
        //    : $"tenants/{CurrentTenant.Id.Value.ToString("D")}/{args.BlobName}";
    }
}