using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class ValidateCpfFunction
{
    [FunctionName("ValidateCpfFunction")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Processing request to validate CPF.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string cpf = data?.cpf;

        if (string.IsNullOrEmpty(cpf) || !IsValidCpf(cpf))
        {
            return new BadRequestObjectResult("Invalid CPF.");
        }

        return new OkObjectResult("CPF is valid.");
    }

    private static bool IsValidCpf(string cpf)
    {
        // Remove non-numeric characters
        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
            return false;

        // Check for invalid CPFs
        string[] invalidCpfs = new string[]
        {
            "00000000000", "11111111111", "22222222222", "33333333333",
            "44444444444", "55555555555", "66666666666", "77777777777",
            "88888888888", "99999999999"
        };
        
        if (invalidCpfs.Contains(cpf))
            return false;

        // Validate first digit
        int[] multiplier1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int sum = 0;

        for (int i = 0; i < 9; i++)
            sum += int.Parse(cpf[i].ToString()) * multiplier1[i];

        int remainder = sum % 11;
        if (remainder < 2)
            remainder = 0;
        else
            remainder = 11 - remainder;

        if (remainder != int.Parse(cpf[9].ToString()))
            return false;

        // Validate second digit
        int[] multiplier2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        sum = 0;

        for (int i = 0; i < 10; i++)
            sum += int.Parse(cpf[i].ToString()) * multiplier2[i];

        remainder = sum % 11;
        if (remainder < 2)
            remainder = 0;
        else
            remainder = 11 - remainder;

        if (remainder != int.Parse(cpf[10].ToString()))
            return false;

        return true;
    }
}
