#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfModelTests : TgStorageTestsBase
{
    #region Methods

    [Test]
    public void TgStorage_App_Constructor()
    {
        Assert.DoesNotThrow(() =>
        {
            TgEfAppEntity app = new();
            TestContext.WriteLine(app);
            using (Assert.EnterMultipleScope())
            {
                app.ApiHash.Should().Be(Guid.Empty);
                app.ApiId.Should().Be(0);
                app.PhoneNumber.Should().Be("+00000000000");
                app.ProxyUid.Should().Be(null);
            }
        });
    }

    [Test]
    public void TgStorage_Document_Constructor()
    {
        Assert.DoesNotThrow(() =>
        {
            TgEfDocumentEntity document = new();
            TestContext.WriteLine(document);
            using (Assert.EnterMultipleScope())
            {
                document.Id.Should().Be((long)0);
                document.SourceId.Should().Be((long)0);
                document.MessageId.Should().Be(0);
                document.FileName.Should().Be(string.Empty);
                document.FileSize.Should().Be((long)0);
                document.AccessHash.Should().Be((long)0);
            }
        });
    }

    [Test]
    public void TgStorage_Filter_Constructor()
    {
        Assert.DoesNotThrow(() =>
        {
            TgEfFilterEntity filter = new();
            TestContext.WriteLine(filter);
            using (Assert.EnterMultipleScope())
            {
                filter.IsEnabled.Should().Be(true);
                filter.FilterType.Should().Be(TgEnumFilterType.SingleName);
                filter.Name.Should().Be("Any");
                filter.Mask.Should().Be("*");
                filter.Size.Should().Be((long)0);
                filter.SizeType.Should().Be(TgEnumFileSizeType.Bytes);
            }
        });
    }

    [Test]
    public void TgStorage_Message_Constructor()
    {
        Assert.DoesNotThrow(() =>
        {
            TgEfMessageEntity message = new();
            TestContext.WriteLine(message);
            using (Assert.EnterMultipleScope())
            {
                message.Id.Should().Be(0);
                message.SourceId.Should().Be((long)0);
                message.Type.Should().Be(TgEnumMessageType.Message);
                message.Size.Should().Be((long)0);
                message.Message.Should().Be(string.Empty);
            }
        });
    }

    [Test]
    public void TgStorage_Proxy_Constructor()
    {
        Assert.DoesNotThrow(() =>
        {
            TgEfProxyEntity proxy = new();
            TestContext.WriteLine(proxy);
            using (Assert.EnterMultipleScope())
            {
                proxy.Type.Should().Be(TgEnumProxyType.None);
                proxy.HostName.Should().Be("No proxy");
                proxy.Port.Should().Be((ushort)404);
                proxy.UserName.Should().Be("No user");
                proxy.Password.Should().Be("No password");
                proxy.Secret.Should().Be(string.Empty);
            }
        });
    }

    [Test]
    public void TgStorage_Source_Constructor()
    {
        Assert.DoesNotThrow(() =>
        {
            TgEfSourceEntity source = new();
            TestContext.WriteLine(source);
            using (Assert.EnterMultipleScope())
            {
                source.Id.Should().Be((long)-1);
                source.UserName.Should().Be("");
                source.Title.Should().Be("");
                source.About.Should().Be("");
                source.Count.Should().Be(1);
            }
        });
    }

    [Test]
    public void TgStorage_Story_Constructor()
    {
        Assert.DoesNotThrow(() =>
        {
            TgEfStoryEntity story = new();
            TestContext.WriteLine(story);
            using (Assert.EnterMultipleScope())
            {
                story.Id.Should().Be((long)-1);
                story.FromId.Should().Be((long)-1);
                story.FromName.Should().Be(string.Empty);
                story.Date.Should().Be(story.GetDefaultPropertyDateTime(nameof(story.Date)));
                story.ExpireDate.Should().Be(story.GetDefaultPropertyDateTime(nameof(story.Date)));
                story.Caption.Should().Be(string.Empty);
                story.Type.Should().Be(string.Empty);
                story.Offset.Should().Be(-1);
                story.Length.Should().Be(-1);
                story.Message.Should().Be(string.Empty);
            }
        });
    }

    [Test]
    public void TgStorage_Version_Constructor()
    {
        Assert.DoesNotThrow(() =>
        {
            TgEfVersionEntity version = new();
            TestContext.WriteLine(version);
            using (Assert.EnterMultipleScope())
            {
                version.Version.Should().Be((short)1024);
                version.Description.Should().Be("New version");
            }
        });
    }

    #endregion
}
