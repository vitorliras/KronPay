namespace Domain.Services.Users;

public interface IProfilePhotoProcessor
{
    ProcessedPhoto Process(byte[] input);
}
