using Domain.Entities.Configuration;
using System.Text;

namespace Infrastructure.AI.Prompts;

public static class TransactionClassificationPrompt
{
    public static string Build(
        string description,
        decimal amount,
        IEnumerable<Category> categories)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are a STRICT financial transaction classifier.");
        sb.AppendLine("You MUST respond with ONLY valid JSON.");
        sb.AppendLine("DO NOT explain.");
        sb.AppendLine("DO NOT add extra text.");
        sb.AppendLine("DO NOT use markdown.");
        sb.AppendLine();

        sb.AppendLine($"Description: \"{description}\"");
        sb.AppendLine($"Amount: {amount}");
        sb.AppendLine();

        sb.AppendLine("Rules:");
        sb.AppendLine("- Investment means applying money expecting future return.");
        sb.AppendLine("- Examples: stocks, ETFs, CDB, LCI, LCA, treasury bonds, crypto, brokers.");
        sb.AppendLine("- If description mentions investment products or brokers, classify as investment.");
        sb.AppendLine("- If unsure, classify as NOT investment and lower confidence.");
        sb.AppendLine("- Normally the description will be portuguese.");
        sb.AppendLine("- If you see the name of a food or supermarket, it's not an investment.");
        sb.AppendLine("- If you see the names of famous stores or cities, especially the city of 'Tijucas', it's not an investment.");
        sb.AppendLine("- The name 'Bra' can be in all descriptions, so it cannot be a reference to know if it's an investment or not.");
        sb.AppendLine("- I need an answer as soon as possible.");
        sb.AppendLine();

        sb.AppendLine("Transaction types:");
        sb.AppendLine("E = Expense");
        sb.AppendLine("I = Income");
        sb.AppendLine("V = Investment");
        sb.AppendLine();

        sb.AppendLine("Format list category:");
        sb.AppendLine($"Id Category|Description category| Transaction types");
        sb.AppendLine();

        sb.AppendLine("Categories:");
        foreach (var c in categories)
        {
            sb.AppendLine($"{c.Id}|{c.Description}|{c.CodTypeTransaction}");
        }

        sb.AppendLine();
        sb.AppendLine("Return EXACT JSON:");
        sb.AppendLine(@"{ 
          ""isInvestment"": true | false,
          ""confidence"": 0.0-1.0,
          ""suggestedCategoryId"": number ,
          ""suggestedType"": ""E"" | ""I"" | ""V""
        }");

        return sb.ToString();
    }
}
