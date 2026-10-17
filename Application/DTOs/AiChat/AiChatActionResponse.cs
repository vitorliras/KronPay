using Application.DTOs.Card.CardPurchases;

namespace Application.DTOs.AiChat;

public sealed record AiChatActionResponse(
    string Type,
    string? LabelKey = null,
    string? Path = null,
    IReadOnlyDictionary<string, string>? QueryParams = null,
    int? CreditCardId = null,
    InvoicePaymentProposalResponse? InvoicePayment = null,
    CardPurchasePreviewResponse? CardPurchasesPreview = null,
    IReadOnlyList<TransactionDraftResponse>? TransactionDrafts = null,
    CategoryDraftResponse? CategoryDraft = null,
    SubcategoryDraftResponse? SubcategoryDraft = null,
    PaymentMethodDraftResponse? PaymentMethodDraft = null,
    FinancialGoalDraftResponse? FinancialGoalDraft = null,
    CategoryBudgetGoalDraftResponse? CategoryBudgetGoalDraft = null,
    GoalContributionDraftResponse? GoalContributionDraft = null,
    PlannedCommitmentDraftResponse? PlannedCommitmentDraft = null);
