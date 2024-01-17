﻿using System.ComponentModel.DataAnnotations;

namespace APICatalogo.Validation
{
    // é necessário utilizar o Attribute quando a classe for utilizada como atributo
    // assim como utilizamos o Controller nos controladores
    public class PrimeiraLetraMaiusculaAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {

            if(value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var primeiraLetra = value.ToString()[0].ToString();
            if (primeiraLetra != primeiraLetra.ToUpper())
            {
                return new ValidationResult("A primeira letra do nome do produto deve ser maiúscula");
            }

            return ValidationResult.Success;
        }
    }
}