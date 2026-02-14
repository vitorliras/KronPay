using Domain.Entities.Configuration;
using System.Text;

namespace Infrastructure.AI.Prompts;

public static class TransactionBatchClassificationPrompt
{
    public static string Build(
        IReadOnlyList<ImportedTransactionResponse> transactions,
        IEnumerable<Category> categories)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are a VERY STRICT financial transaction classifier.");
        sb.AppendLine("You MUST return ONLY valid JSON.");
        sb.AppendLine("NO explanations.");
        sb.AppendLine("NO markdown.");
        sb.AppendLine("NO extra text.");
        sb.AppendLine();

        sb.AppendLine("You will receive a LIST of transactions.");
        sb.AppendLine("You MUST return ONE JSON OBJECT PER TRANSACTION.");
        sb.AppendLine("The output array size MUST be EXACTLY the same as input.");
        sb.AppendLine("The order MUST be preserved.");
        sb.AppendLine();

        sb.AppendLine("INPUT TRANSACTIONS:");
        sb.AppendLine();

        var index = 1;
        foreach (var t in transactions)
        {
            sb.AppendLine($"[{index}] Description: \"{t.Description}\"");
            sb.AppendLine($"[{index}] Amount: {t.Amount}");
            sb.AppendLine();
            index++;
        }

        sb.AppendLine("STRICT RULES:");
        sb.AppendLine("- Investment = applying money expecting future return.");
        sb.AppendLine("- ONLY investments: stocks, ETFs, CDB, LCI, LCA, treasury bonds, crypto, brokers.");
        sb.AppendLine("- Supermarkets, food, stores, cities, services = NOT investment.");
        sb.AppendLine("- City names like 'Tijucas' are NOT investments.");
        sb.AppendLine("- The word 'BRA' means nothing.");
        sb.AppendLine("- If NOT 100% sure, set isInvestment = false.");
        sb.AppendLine("- Do NOT guess investments.");
        sb.AppendLine();

        sb.AppendLine("Transaction types:");
        sb.AppendLine("E = Expense");
        sb.AppendLine("I = Income");
        sb.AppendLine("V = Investment");
        sb.AppendLine();

        sb.AppendLine("CATEGORIES (use ONLY these IDs):");
        sb.AppendLine("Format text: ID|description category|type transaction");

        foreach (var c in categories)
        {
            sb.AppendLine($"{c.Id}|{c.Description}|{c.CodTypeTransaction}");
        }

        sb.AppendLine();
        sb.AppendLine("RETURN EXACTLY THIS JSON FORMAT:");
        sb.AppendLine();

        sb.AppendLine(@"[
          {
            ""index"": 1,
            ""isInvestment"": false,
            ""confidence"": 0.75,
            ""suggestedCategoryId"": 5,
            ""suggestedType"": ""E""
          }
        ]");

        sb.AppendLine();
        sb.AppendLine("FINAL RULES:");
        sb.AppendLine("- index MUST match input index");
        sb.AppendLine("- confidence MUST be between 0.0 and 1.0 (decimal)");
        sb.AppendLine("- suggestedType MUST be E, I or V");
        sb.AppendLine("- NEVER skip an index");
        sb.AppendLine("- NEVER change order");
        sb.AppendLine("- See which category best fits each transaction.");
        sb.AppendLine("- All transactions must have a category.");
        sb.AppendLine("- A transaction of this type can only have one category of the same type.");
        sb.AppendLine("- TThe exact number of JSON objects in the array should be "+ transactions.Count());

        return sb.ToString();

    }
}
