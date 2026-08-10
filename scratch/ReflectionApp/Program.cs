using System;
using System.Linq;
using System.Reflection;
using Fido2NetLib;
using Fido2NetLib.Objects;

class Program
{
    static void Main()
    {
        var type = typeof(IFido2);
        Console.WriteLine("IFido2 Methods:");
        foreach (var method in type.GetMethods())
        {
            Console.WriteLine($"{method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");
        }

        Console.WriteLine("\nAuthenticatorAssertionRawResponse Properties:");
        foreach (var prop in typeof(AuthenticatorAssertionRawResponse).GetProperties())
        {
            Console.WriteLine($"{prop.PropertyType.Name} {prop.Name}");
        }

        Console.WriteLine("\nTesting Deserialization:");
        var testJson1 = @"{
            ""id"": ""testId"",
            ""rawId"": ""dGVzdFJhd0lk"",
            ""type"": ""public-key"",
            ""response"": {
                ""attestationObject"": ""dGVzdEF0dGVzdGF0aW9uT2JqZWN0"",
                ""clientDataJSON"": ""dGVzdENsaWVudERhdGFKU09O""
            }
        }";

        var testJson2 = @"{
            ""id"": ""testId"",
            ""rawId"": ""dGVzdFJhd0lk"",
            ""type"": ""public-key"",
            ""response"": {
                ""attestationObject"": ""dGVzdEF0dGVzdGF0aW9uT2JqZWN0"",
                ""clientDataJson"": ""dGVzdENsaWVudERhdGFKU09O""
            }
        }";

        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        try
        {
            var res1 = System.Text.Json.JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(testJson1, options);
            Console.WriteLine($"testJson1 (clientDataJSON): Response is null? {res1?.Response == null}, AttestationObject null? {res1?.Response?.AttestationObject == null}, ClientDataJson null? {res1?.Response?.ClientDataJson == null}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"testJson1 failed: {ex.Message}");
        }

        Console.WriteLine("\nTesting Validation on Full JSON payload:");
        var fullJson = @"{
            ""id"": ""testId"",
            ""rawId"": ""dGVzdFJhd0lk"",
            ""type"": ""public-key"",
            ""clientExtensionResults"": {},
            ""response"": {
                ""attestationObject"": ""dGVzdEF0dGVzdGF0aW9uT2JqZWN0"",
                ""clientDataJSON"": ""dGVzdENsaWVudERhdGFKU09O""
            }
        }";
        var fullObj = System.Text.Json.JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(fullJson, options);
        var respObj = new Fido2NetLib.AuthenticatorAttestationRawResponse.ResponseBase();
        var ctx3 = new System.ComponentModel.DataAnnotations.ValidationContext(respObj);
        var res3 = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(respObj, ctx3, res3, true);
        
        var props = typeof(AuthenticatorAttestationRawResponse.ResponseBase).GetProperties();
        Console.WriteLine("\nResponseBase Properties:");
        foreach(var p in props) Console.WriteLine($"{p.PropertyType.Name} {p.Name}");

        var rootProps = typeof(AuthenticatorAttestationRawResponse).GetProperties();
        Console.WriteLine("\nRoot Properties:");
        foreach(var p in rootProps) Console.WriteLine($"{p.PropertyType.Name} {p.Name}");
        
        if (res2.Count == 0) {
            Console.WriteLine("Validation passed!");
        }
    }
}
