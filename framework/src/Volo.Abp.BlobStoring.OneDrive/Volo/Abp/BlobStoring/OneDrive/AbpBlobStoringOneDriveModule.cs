using Volo.Abp.Modularity;

namespace Volo.Abp.BlobStoring.OneDrive;

[DependsOn(typeof(AbpBlobStoringModule))]
public class AbpBlobStoringOneDriveModule : AbpModule
{

}