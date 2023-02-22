
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validations;

namespace WebApiAutores.test.PruebasUnitarias
{
    [TestClass]
    public class FirstUppercaseAttributeTest
    {
        [TestMethod]
        public void PrimeraLetraMinuscula_ReturnError()
        {
            //Preparation
            var FirstLetterUpper = new FirstUppercaseAttribute();
            var value = "jhon";
            var context = new ValidationContext(new { Nombre = value });

            //Ejecution

            var result = FirstLetterUpper.GetValidationResult(value, context);

            //Verification
            Assert.AreEqual("the first letter must be capitalized", result.ErrorMessage);
        }

        [TestMethod]
        public void NullValue_NotReturnError()
        {
            //Preparation
            var FirstLetterUpper = new FirstUppercaseAttribute();
            string value = null;
            var context = new ValidationContext(new { Nombre = value });

            //Ejecution

            var result = FirstLetterUpper.GetValidationResult(value, context);

            //Verification
            Assert.IsNull(result);
        }

        [TestMethod]
        public void PrimeraLetraMayuscula_NotReturnError()
        {
            //Preparation
            var FirstLetterUpper = new FirstUppercaseAttribute();
            var value = "Jhon";
            var context = new ValidationContext(new { Nombre = value });

            //Ejecution

            var result = FirstLetterUpper.GetValidationResult(value, context);

            //Verification
            Assert.IsNull(result);
        }
    }
}