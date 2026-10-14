namespace Practica_Ing_de_software_I_.Application.DTOs;

public record CreateReviewRequest(
    int ProjectId,
    string Comments,
    string NewStatus
);

public record SubmitReviewRequest(
    string Comments,
    string NewStatus
);

public record ReviewResponse(
    int ProjectId,
    string Comments,
    string Status,
    DateTime ReviewedAt
);
