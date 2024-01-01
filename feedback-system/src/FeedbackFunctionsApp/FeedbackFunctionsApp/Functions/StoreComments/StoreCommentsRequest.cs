
using FeedbackFunctionsApp.Exceptions;

namespace FeedbackFunctionsApp.Functions;

public record StoreCommentsRequest(string Comments);

public class StoreCommentsRequestDto
{ 
    public string? Comments { get; set; }

    public StoreCommentsRequest GenerateValidObject()
    {
        if (string.IsNullOrWhiteSpace(Comments))
        {
            throw new BadRequestException("Request missing property: Comments");
        }

        return new StoreCommentsRequest(Comments);
    }
}
