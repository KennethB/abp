using System;

using Shouldly;

using Volo.Abp.BlobStoring.OneDrive;
using Volo.Abp.MultiTenancy;

using Xunit;

namespace Volo.Abp.BlobStoring.OneDrive;


public class OneDriveBlobNameCalculator_Tests : AbpBlobStoringOneDriveTestCommonBase
{
    private readonly IOneDriveBlobNameCalculator _calculator;
    private readonly ICurrentTenant _currentTenant;

    private const string OneDriveContainerName = "/";
    private const string OneDriveSeparator = "/";

    public OneDriveBlobNameCalculator_Tests()
    {
        _calculator = GetRequiredService<IOneDriveBlobNameCalculator>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public void Default_Settings()
    {
        _calculator.Calculate(
            GetArgs("my-container", "my-blob")
        ).ShouldBe($"host{OneDriveSeparator}my-blob");
    }

    [Fact]
    public void Default_Settings_With_TenantId()
    {
        var tenantId = Guid.NewGuid();

        using (_currentTenant.Change(tenantId))
        {
            _calculator.Calculate(
                GetArgs("my-container", "my-blob")
            ).ShouldBe($"tenants{OneDriveSeparator}{tenantId:D}{OneDriveSeparator}my-blob");
        }
    }

    private static BlobProviderArgs GetArgs(
        string containerName,
        string blobName)
    {
        return new BlobProviderGetArgs(
            containerName,
            new BlobContainerConfiguration().UseOneDrive(x =>
            {
                x.ContainerName = containerName;
            }),
            blobName
        );
    }
}