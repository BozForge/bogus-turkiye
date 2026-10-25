using System.Text.RegularExpressions;
using Bogus;
using TurkishId;
using TurkishValidators.Validators;
using IbanValidator = IbanNet.IbanValidator;

namespace BozForge.Bogus.Turkiye.Tests;

// Üretilen değerler kendi kodumuzla değil, bağımsız kütüphanelerle doğrulanır:
// TCKN için TurkishId ve TurkishValidators, VKN için TurkishValidators, IBAN için IbanNet.
public class ExtensionsForTurkiyeTests
{
    private const int Samples = 2_000;

    private static Faker NewFaker(int seed = 2026) => new("tr") { Random = new Randomizer(seed) };

    [Fact]
    public void Tckn_passes_independent_validators()
    {
        for (var i = 0; i < Samples; i++)
        {
            var tckn = new Person("tr", seed: i).Tckn();

            Assert.Equal(11, tckn.Length);
            Assert.NotEqual('0', tckn[0]);
            Assert.True(TurkishIdNumber.IsValid(tckn), $"TurkishId reddetti: {tckn}");
            Assert.True(TcKimlikNoValidator.IsValid(tckn), $"TurkishValidators reddetti: {tckn}");
        }
    }

    [Fact]
    public void Tckn_is_stable_for_the_same_person()
    {
        var person = NewFaker().Person;
        Assert.Equal(person.Tckn(), person.Tckn());
    }

    [Fact]
    public void Tckn_is_repeatable_with_the_same_seed()
    {
        var first = new Faker<Kisi>("tr").UseSeed(7).RuleFor(k => k.Tckn, f => f.Person.Tckn()).Generate(20);
        var second = new Faker<Kisi>("tr").UseSeed(7).RuleFor(k => k.Tckn, f => f.Person.Tckn()).Generate(20);

        Assert.Equal(first.Select(k => k.Tckn), second.Select(k => k.Tckn));
        Assert.Equal(20, first.Select(k => k.Tckn).Distinct().Count());
    }

    [Theory]
    [InlineData("172917160", "60")]   // python-stdnum belgelerindeki örnek
    [InlineData("100000001", "46")]   // TurkishValidators belgelerindeki örnek
    public void Tckn_check_digits_match_known_numbers(string first9, string expected)
    {
        var (tenth, eleventh) = Checksums.TcknCheckDigits(first9.Select(c => c - '0').ToArray());
        Assert.Equal(expected, $"{tenth}{eleventh}");
    }

    [Fact]
    public void Vkn_passes_independent_validator()
    {
        var f = NewFaker();
        for (var i = 0; i < Samples; i++)
        {
            var vkn = f.Company.Vkn();

            Assert.Equal(10, vkn.Length);
            Assert.True(VergiNoValidator.IsValid(vkn), $"TurkishValidators reddetti: {vkn}");
        }
    }

    [Fact]
    public void Vkn_check_digit_matches_known_number()
    {
        // python-stdnum belgelerindeki geçerli örnek: 4540536920
        Assert.Equal(0, Checksums.VknCheckDigit("454053692".Select(c => c - '0').ToArray()));
    }

    [Fact]
    public void TrIban_passes_ibannet_and_turkish_rules()
    {
        var f = NewFaker();
        var validator = new IbanValidator();

        for (var i = 0; i < Samples; i++)
        {
            var iban = f.Finance.TrIban();

            Assert.Matches("^TR[0-9]{24}$", iban);
            Assert.Equal('0', iban[9]);
            Assert.True(validator.Validate(iban).IsValid, $"IbanNet reddetti: {iban}");
            Assert.True(TurkishValidators.Validators.IbanValidator.IsValid(iban), $"TurkishValidators reddetti: {iban}");

            var bankCode = int.Parse(iban.Substring(4, 5));
            Assert.True(Enum.IsDefined(typeof(TrBank), bankCode), $"Bilinmeyen banka kodu: {bankCode}");
        }
    }

    [Theory]
    [InlineData(TrBank.Ziraat, "00010")]
    [InlineData(TrBank.Akbank, "00046")]
    [InlineData(TrBank.Garanti, "00062")]
    [InlineData(TrBank.KuveytTurk, "00205")]
    public void TrIban_uses_the_requested_bank_code(TrBank bank, string code)
    {
        var iban = NewFaker().Finance.TrIban(bank);
        Assert.Equal(code, iban.Substring(4, 5));
    }

    [Fact]
    public void TrIban_is_valid_for_every_bank()
    {
        var f = NewFaker();
        var validator = new IbanValidator();
        foreach (TrBank bank in Enum.GetValues(typeof(TrBank)))
        {
            var iban = f.Finance.TrIban(bank);
            Assert.True(validator.Validate(iban).IsValid, $"{bank}: {iban}");
        }
    }

    [Fact]
    public void TrIban_formatted_groups_by_four()
    {
        var iban = NewFaker().Finance.TrIban(formatted: true);

        Assert.Matches("^TR[0-9]{2}( [0-9]{4}){5} [0-9]{2}$", iban);
        // IbanNet katı modda boşluk kabul etmez; biçim yalnızca görüntü içindir.
        Assert.True(new IbanValidator().Validate(iban.Replace(" ", "")).IsValid);
    }

    [Fact]
    public void TrTestCard_numbers_are_luhn_valid_and_from_the_public_list()
    {
        var f = NewFaker();
        var known = TestCardData.All.Select(c => c.Number).ToHashSet();

        for (var i = 0; i < 500; i++)
        {
            var card = f.Finance.TrTestCard();

            Assert.Contains(card.Number, known);
            Assert.True(Checksums.IsValidLuhn(card.Number), card.Number);
            Assert.InRange(card.ExpiryMonth, 1, 12);
            Assert.True(card.ExpiryYear > DateTime.UtcNow.Year);
            Assert.Equal(card.Scheme == CardScheme.Amex ? 4 : 3, card.Cvv.Length);
        }
    }

    [Fact]
    public void Every_public_test_card_is_luhn_valid()
    {
        Assert.All(TestCardData.All, c => Assert.True(Checksums.IsValidLuhn(c.Number), c.Number));
    }

    [Theory]
    [InlineData(CardScheme.Troy, null)]
    [InlineData(CardScheme.Visa, CardType.Debit)]
    [InlineData(CardScheme.Amex, CardType.Credit)]
    public void TrTestCard_respects_filters(CardScheme scheme, CardType? type)
    {
        var f = NewFaker();
        for (var i = 0; i < 50; i++)
        {
            var card = f.Finance.TrTestCard(scheme, type);
            Assert.Equal(scheme, card.Scheme);
            if (type is not null) Assert.Equal(type, card.Type);
        }
    }

    [Fact]
    public void TrTestCard_throws_when_no_card_matches()
    {
        Assert.Throws<ArgumentException>(() => NewFaker().Finance.TrTestCard(CardScheme.Amex, CardType.Debit));
    }

    [Theory]
    [InlineData(TrPhoneFormat.E164, @"^\+905[0-9]{9}$")]
    [InlineData(TrPhoneFormat.International, @"^\+90 5[0-9]{2} [0-9]{3} [0-9]{2} [0-9]{2}$")]
    [InlineData(TrPhoneFormat.National, @"^05[0-9]{2} [0-9]{3} [0-9]{2} [0-9]{2}$")]
    [InlineData(TrPhoneFormat.Digits, @"^5[0-9]{9}$")]
    public void TrGsm_matches_format(TrPhoneFormat format, string pattern)
    {
        var f = NewFaker();
        for (var i = 0; i < 200; i++)
        {
            var gsm = f.Phone.TrGsm(format);
            Assert.Matches(pattern, gsm);
        }
    }

    [Fact]
    public void TrGsm_passes_independent_validator()
    {
        var f = NewFaker();
        for (var i = 0; i < 500; i++)
        {
            var gsm = f.Phone.TrGsm(TrPhoneFormat.Digits);
            Assert.True(TelefonNoValidator.IsValid(gsm), $"TurkishValidators reddetti: {gsm}");
        }
    }

    private sealed class Kisi
    {
        public string Tckn { get; set; } = "";
    }
}
