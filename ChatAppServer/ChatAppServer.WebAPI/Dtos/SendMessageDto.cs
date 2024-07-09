namespace ChatAppServer.WebAPI.Dtos;

public sealed record SendMessageDto(
    Guid FromUserId,
    Guid ToUserId,
    string Message);