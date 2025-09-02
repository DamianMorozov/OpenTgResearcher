// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructureTests.Helpers;

[TestFixture]
public class TgStringUtilsTests
{
    [TestCase("username", true, ExpectedResult = "@username")]
    [TestCase("@username", true, ExpectedResult = "@username")]
    [TestCase("https://t.me/username", true, ExpectedResult = "@username")]
    [TestCase("https://t.me/username", false, ExpectedResult = "username")]
    [TestCase("@username", false, ExpectedResult = "username")]
    [TestCase("username", false, ExpectedResult = "username")]
    public string NormalizedTgName_ReturnsExpected(string input, bool isAddAt)
    {
        return TgStringUtils.NormalizedTgName(input, isAddAt);
    }

    [TestCase("UserOne", true, new[] { "@userone" })]
    [TestCase("UserOne", false, new[] { "userone" })]
    [TestCase("UserOne,UserTwo,UserThree", true, new[] { "@userone", "@usertwo", "@userthree" })]
    [TestCase("UserOne,UserTwo,UserThree", false, new[] { "userone", "usertwo", "userthree" })]
    [TestCase("UserOne; UserTwo UserThree", true, new[] { "@userone", "@usertwo", "@userthree" })]
    [TestCase("UserOne; UserTwo UserThree", false, new[] { "userone", "usertwo", "userthree" })]
    [TestCase(" UserOne ,  UserTwo;UserThree ", true, new[] { "@userone", "@usertwo", "@userthree" })]
    [TestCase(" UserOne ,  UserTwo;UserThree ", false, new[] { "userone", "usertwo", "userthree" })]
    [TestCase("", true, new string[0])]
    [TestCase("", false, new string[0])]
    public void NormalizedTgNames_ReturnsExpected(string input, bool isAddAt, string[] expected)
    {
        var result = TgStringUtils.NormalizedTgNames(input, isAddAt);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void NormalizedTgNames_SingleName_AddAt()
    {
        var result = TgStringUtils.NormalizedTgNames("UserOne", isAddAt: true);
        Assert.That(result, Is.EqualTo(new List<string> { "@userone" }));
    }

    [Test]
    public void NormalizedTgNames_SingleName_NoAddAt()
    {
        var result = TgStringUtils.NormalizedTgNames("UserOne", isAddAt: false);
        Assert.That(result, Is.EqualTo(new List<string> { "userone" }));
    }

    [Test]
    public void NormalizedTgNames_CommaSeparated_AddAt()
    {
        var result = TgStringUtils.NormalizedTgNames("UserOne,UserTwo,UserThree", isAddAt: true);
        Assert.That(result, Is.EqualTo(new List<string> { "@userone", "@usertwo", "@userthree" }));
    }

    [Test]
    public void NormalizedTgNames_CommaSeparated_NoAddAt()
    {
        var result = TgStringUtils.NormalizedTgNames("UserOne,UserTwo,UserThree", isAddAt: false);
        Assert.That(result, Is.EqualTo(new List<string> { "userone", "usertwo", "userthree" }));
    }

    [Test]
    public void NormalizedTgNames_SemicolonAndSpaceSeparated_AddAt()
    {
        var result = TgStringUtils.NormalizedTgNames("UserOne; UserTwo UserThree", isAddAt: true);
        Assert.That(result, Is.EqualTo(new List<string> { "@userone", "@usertwo", "@userthree" }));
    }

    [Test]
    public void NormalizedTgNames_SemicolonAndSpaceSeparated_NoAddAt()
    {
        var result = TgStringUtils.NormalizedTgNames("UserOne; UserTwo UserThree", isAddAt: false);
        Assert.That(result, Is.EqualTo(new List<string> { "userone", "usertwo", "userthree" }));
    }

    [Test]
    public void NormalizedTgNames_MixedSeparatorsAndSpaces_AddAt()
    {
        var result = TgStringUtils.NormalizedTgNames(" UserOne ,  UserTwo;UserThree ", isAddAt: true);
        Assert.That(result, Is.EqualTo(new List<string> { "@userone", "@usertwo", "@userthree" }));
    }

    [Test]
    public void NormalizedTgNames_MixedSeparatorsAndSpaces_NoAddAt()
    {
        var result = TgStringUtils.NormalizedTgNames(" UserOne ,  UserTwo;UserThree ", isAddAt: false);
        Assert.That(result, Is.EqualTo(new List<string> { "userone", "usertwo", "userthree" }));
    }

    [Test]
    public void NormalizedTgNames_EmptyString()
    {
        var result = TgStringUtils.NormalizedTgNames("");
        Assert.That(result, Is.EqualTo(new List<string>()));
    }
}
