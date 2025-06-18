// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructureTest.Helpers;

[TestFixture]
public class TgStringUtilsTests
{
    [TestCase("username", true, ExpectedResult = "@username")]
    [TestCase("@username", true, ExpectedResult = "@username")]
    [TestCase("https://t.me/username", true, ExpectedResult = "@username")]
    [TestCase("https://t.me/username", false, ExpectedResult = "username")]
    [TestCase("@username", false, ExpectedResult = "username")]
    [TestCase("username", false, ExpectedResult = "username")]
    public string NormilizeTgName_ReturnsExpected(string input, bool isAddAt)
    {
        return TgStringUtils.NormilizeTgName(input, isAddAt);
    }

    [Test]
    public void NormilizeTgNames_SingleName()
    {
        var result = TgStringUtils.NormilizeTgNames("UserOne");
        Assert.That(result, Is.EqualTo(new List<string> { "userone" }));
    }

    [Test]
    public void NormilizeTgNames_CommaSeparated()
    {
        var result = TgStringUtils.NormilizeTgNames("UserOne,UserTwo,UserThree");
        Assert.That(result, Is.EqualTo(new List<string> { "userone", "usertwo", "userthree" }));
    }

    [Test]
    public void NormilizeTgNames_SemicolonAndSpaceSeparated()
    {
        var result = TgStringUtils.NormilizeTgNames("UserOne; UserTwo UserThree");
        Assert.That(result, Is.EqualTo(new List<string> { "userone", "usertwo", "userthree" }));
    }

    [Test]
    public void NormilizeTgNames_MixedSeparatorsAndSpaces()
    {
        var result = TgStringUtils.NormilizeTgNames(" UserOne ,  UserTwo;UserThree ");
        Assert.That(result, Is.EqualTo(new List<string> { "userone", "usertwo", "userthree" }));
    }

    [Test]
    public void NormilizeTgNames_EmptyString()
    {
        var result = TgStringUtils.NormilizeTgNames("");
        Assert.That(result, Is.EqualTo(new List<string>()));
    }
}
