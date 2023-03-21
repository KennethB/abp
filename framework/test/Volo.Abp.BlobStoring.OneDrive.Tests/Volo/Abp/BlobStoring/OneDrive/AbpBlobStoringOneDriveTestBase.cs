using Volo.Abp.BlobStoring.OneDrive;
using Volo.Abp.Testing;

namespace Volo.Abp.BlobStoring.OneDrive
{

    public class AbpBlobStoringOneDriveTestCommonBase : AbpIntegratedTest<AbpBlobStoringOneDriveTestCommonModule>
    {
        protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
        {
            options.UseAutofac();
        }
    }

    public class AbpBlobStoringOneDriveTestBase : AbpIntegratedTest<AbpBlobStoringOneDriveTestModule>
    {
        protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
        {
            options.UseAutofac();
        }
    }
}