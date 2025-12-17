using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace StudentHelperTelegramBot.Models
{
    // Auth responses
    public record AuthResponse(
        [property: JsonPropertyName("token")] string? Token,
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("message")] string Message);

    public record RegistrationResponse(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("token")] string? Token);

    public record AddPublicationResponse(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("succuss")] bool Success,
        [property: JsonPropertyName("message")] string Message);

    public record AddGroupResponse(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("message")] string Message);

    public record SendMessageResponse(
        [property: JsonPropertyName("answer")] string Answer,
        [property: JsonPropertyName("conversationId")] string ConversationId,
        [property: JsonPropertyName("createdAt")] DateTime CreatedAt);

    public record Result<T>(
        bool IsSuccess,
        T? Value,
        string? Error);

    public record ReadPublicationsResponse(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("publications")] List<PublicationResponse>? Publications);

    public record GetPublicationResponse(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("publication")] PublicationResponse? Publication);

    public record PublicationResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("publicationType")] string PublicationType,
        [property: JsonPropertyName("authorId")] string AuthorId,
        [property: JsonPropertyName("groupId")] string? GroupId,
        [property: JsonPropertyName("isPublished")] bool IsPublished,
        [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
        [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt);

    public record Lecture(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("externalUrl")] string ExternalUrl,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("createdAt")] DateTime CreatedAt);

    public record AddLectureResponse(
        [property: JsonPropertyName("id")] Guid? Id,
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("message")] string Message);

    public record UserInfoResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("role")] string Role);
}