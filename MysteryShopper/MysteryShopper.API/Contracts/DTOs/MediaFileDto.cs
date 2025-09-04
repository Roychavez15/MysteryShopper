using MysteryShopper.API.Domain;

namespace MysteryShopper.API.Contracts.DTOs
{
    //public record MediaFileDto(Guid Id, string Path, string Type);
    public record MediaFileDto(Guid Id, string FileName, string RelativePath, MediaKind Kind, long SizeBytes, Guid? ResponseId, Guid? AnswerId);

}
