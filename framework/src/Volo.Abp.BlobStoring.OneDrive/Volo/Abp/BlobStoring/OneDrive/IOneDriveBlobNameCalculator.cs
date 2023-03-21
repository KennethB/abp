namespace Volo.Abp.BlobStoring.OneDrive;

public interface IOneDriveBlobNameCalculator
{
    string Calculate(BlobProviderArgs args);
}