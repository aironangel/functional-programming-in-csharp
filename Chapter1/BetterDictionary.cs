using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;

namespace Chapter1
{

    internal interface IValidator<T, TResult>
    {
        TResult IsValid(T toValidate);
    }

    internal static class DictionaryExtension
    {
        internal static TResult GetSafe<TKey, TValidator, TToValidate, TResult>(this Dictionary<TKey, TValidator> dictionary, TKey key, TToValidate toValidate, TResult faultBackValue)
            where TValidator : IValidator<TToValidate, TResult> => 
            dictionary.ContainsKey(key) ? dictionary[key].IsValid(toValidate) : faultBackValue;

        internal static TResult GetSafe<TKey, TResult>(this Dictionary<TKey, TResult> dictionary, TKey key, TResult faultBack) =>
            dictionary.ContainsKey(key) ? dictionary[key] : faultBack;


    }

    public class BetterDictionary
    {
        [Fact]
        public void better_dictionary()
        {
            Dictionary<int, string> sut = new Dictionary<int, string>();
            sut.Add(10, "ten");
            sut.Add(20, "twenty");

            sut.GetSafe(10, "missing").Should().Be("ten");
            sut.GetSafe(1, "missing").Should().Be("missing");
        }

        [Fact]
        public void better_dictionary_ivalidator()
        {
            IValidator<string, bool> validatorCardNum = Substitute.For<IValidator<string, bool>>();
            validatorCardNum.IsValid("12345678").Returns(true);
            validatorCardNum.IsValid("00000000").Returns(false);

            IValidator<string, bool> validatorCoupon = Substitute.For<IValidator<string, bool>>();
            validatorCoupon.IsValid("12345678012").Returns(true);
            validatorCoupon.IsValid("00000000000").Returns(false);

            IValidator<string, bool> falseFaultBack = Substitute.For<IValidator<string, bool>>();
            falseFaultBack.IsValid("").ReturnsForAnyArgs(false);

            Dictionary<int, IValidator<string, bool>> sut = new Dictionary<int, IValidator<string, bool>>();
            sut.Add(8, validatorCardNum);
            sut.Add(12, validatorCoupon);

            sut.GetSafe("12345678".Length, falseFaultBack).Should().Be(true);
            sut.GetSafe("00000000".Length, falseFaultBack).Should().Be(false);
            sut.GetSafe("123456789012".Length, falseFaultBack).Should().Be(true);
            sut.GetSafe("000000000000".Length, falseFaultBack).Should().Be(false);
            sut.GetSafe("123".Length, falseFaultBack).Should().Be(false);
            sut.GetSafe("000".Length, falseFaultBack).Should().Be(false);
        }

    }
}
