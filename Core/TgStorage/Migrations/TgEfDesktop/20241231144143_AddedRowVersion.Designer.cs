﻿// <auto-generated />
#nullable disable

namespace TgStorage.Migrations.TgEfDesktop
{
    [DbContext(typeof(TgEfDesktopContext))]
    [Migration("20241231144143_AddedRowVersion")]
    partial class AddedRowVersion
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("TgStorage.Domain.Apps.TgEfAppEntity", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("UID");

                    b.Property<Guid>("ApiHash")
                        .IsConcurrencyToken()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("API_HASH");

                    b.Property<int>("ApiId")
                        .IsConcurrencyToken()
                        .HasColumnType("INT")
                        .HasColumnName("API_ID");

                    b.Property<string>("FirstName")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("NVARCHAR(64)")
                        .HasColumnName("FIRST_NAME");

                    b.Property<string>("LastName")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("NVARCHAR(64)")
                        .HasColumnName("LAST_NAME");

                    b.Property<string>("PhoneNumber")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("NVARCHAR(20)")
                        .HasColumnName("PHONE_NUMBER");

                    b.Property<Guid?>("ProxyUid")
                        .IsConcurrencyToken()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("PROXY_UID");

                    b.HasKey("Uid");

                    b.HasIndex("ApiHash")
                        .IsUnique();

                    b.HasIndex("ApiId");

                    b.HasIndex("PhoneNumber");

                    b.HasIndex("ProxyUid");

                    b.HasIndex("Uid")
                        .IsUnique();

                    b.ToTable("APPS", (string)null);
                });

            modelBuilder.Entity("TgStorage.Domain.Contacts.TgEfContactEntity", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("UID");

                    b.Property<long>("AccessHash")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("ACCESS_HASH");

                    b.Property<int>("BotActiveUsers")
                        .IsConcurrencyToken()
                        .HasColumnType("INT(20)")
                        .HasColumnName("BOT_ACTIVE_USERS");

                    b.Property<string>("BotInfoVersion")
                        .IsConcurrencyToken()
                        .HasMaxLength(20)
                        .HasColumnType("NVARCHAR(20)")
                        .HasColumnName("BOT_INFO_VERSION");

                    b.Property<string>("BotInlinePlaceholder")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("BOT_INLINE_PLACEHOLDER");

                    b.Property<DateTime>("DtChanged")
                        .IsConcurrencyToken()
                        .HasColumnType("DATETIME")
                        .HasColumnName("DT_CHANGED");

                    b.Property<string>("FirstName")
                        .IsConcurrencyToken()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("FIRST_NAME");

                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("ID");

                    b.Property<bool>("IsActive")
                        .IsConcurrencyToken()
                        .HasColumnType("BIT")
                        .HasColumnName("IS_ACTIVE");

                    b.Property<bool>("IsBot")
                        .IsConcurrencyToken()
                        .HasColumnType("BIT")
                        .HasColumnName("IS_BOT");

                    b.Property<string>("LangCode")
                        .IsConcurrencyToken()
                        .HasMaxLength(16)
                        .HasColumnType("NVARCHAR(16)")
                        .HasColumnName("LANG_CODE");

                    b.Property<string>("LastName")
                        .IsConcurrencyToken()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("LAST_NAME");

                    b.Property<string>("PhoneNumber")
                        .IsConcurrencyToken()
                        .HasMaxLength(20)
                        .HasColumnType("NVARCHAR(20)")
                        .HasColumnName("PHONE_NUMBER");

                    b.Property<string>("RestrictionReason")
                        .IsConcurrencyToken()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("RESTRICTION_REASON");

                    b.Property<string>("Status")
                        .IsConcurrencyToken()
                        .HasMaxLength(20)
                        .HasColumnType("NVARCHAR(20)")
                        .HasColumnName("STATUS");

                    b.Property<int>("StoriesMaxId")
                        .IsConcurrencyToken()
                        .HasColumnType("INT(20)")
                        .HasColumnName("STORIES_MAX_ID");

                    b.Property<string>("UserName")
                        .IsConcurrencyToken()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("USER_NAME");

                    b.Property<string>("UserNames")
                        .IsConcurrencyToken()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("USER_NAMES");

                    b.HasKey("Uid");

                    b.HasIndex("AccessHash");

                    b.HasIndex("DtChanged");

                    b.HasIndex("FirstName");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("IsActive");

                    b.HasIndex("IsBot");

                    b.HasIndex("LangCode");

                    b.HasIndex("LastName");

                    b.HasIndex("PhoneNumber");

                    b.HasIndex("Status");

                    b.HasIndex("Uid")
                        .IsUnique();

                    b.HasIndex("UserName");

                    b.ToTable("CONTACTS", (string)null);
                });

            modelBuilder.Entity("TgStorage.Domain.Documents.TgEfDocumentEntity", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("UID");

                    b.Property<long>("AccessHash")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("ACCESS_HASH");

                    b.Property<string>("FileName")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("NVARCHAR(256)")
                        .HasColumnName("FILE_NAME");

                    b.Property<long>("FileSize")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("FILE_SIZE");

                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("ID");

                    b.Property<long>("MessageId")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("MESSAGE_ID");

                    b.Property<long?>("SourceId")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("SOURCE_ID");

                    b.HasKey("Uid");

                    b.HasIndex("AccessHash");

                    b.HasIndex("FileName");

                    b.HasIndex("FileSize");

                    b.HasIndex("Id");

                    b.HasIndex("MessageId");

                    b.HasIndex("SourceId");

                    b.HasIndex("Uid")
                        .IsUnique();

                    b.ToTable("DOCUMENTS", (string)null);
                });

            modelBuilder.Entity("TgStorage.Domain.Filters.TgEfFilterEntity", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("UID");

                    b.Property<int>("FilterType")
                        .IsConcurrencyToken()
                        .HasColumnType("INT")
                        .HasColumnName("FILTER_TYPE");

                    b.Property<bool>("IsEnabled")
                        .IsConcurrencyToken()
                        .HasColumnType("BIT")
                        .HasColumnName("IS_ENABLED");

                    b.Property<string>("Mask")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("MASK");

                    b.Property<string>("Name")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("NAME");

                    b.Property<long>("Size")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("SIZE");

                    b.Property<int>("SizeType")
                        .IsConcurrencyToken()
                        .HasColumnType("INT")
                        .HasColumnName("SIZE_TYPE");

                    b.HasKey("Uid");

                    b.HasIndex("FilterType");

                    b.HasIndex("IsEnabled");

                    b.HasIndex("Mask");

                    b.HasIndex("Name");

                    b.HasIndex("Size");

                    b.HasIndex("SizeType");

                    b.HasIndex("Uid")
                        .IsUnique();

                    b.ToTable("FILTERS", (string)null);
                });

            modelBuilder.Entity("TgStorage.Domain.Messages.TgEfMessageEntity", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("UID");

                    b.Property<DateTime>("DtCreated")
                        .IsConcurrencyToken()
                        .HasColumnType("DATETIME")
                        .HasColumnName("DT_CREATED");

                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("ID");

                    b.Property<string>("Message")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasColumnType("NVARCHAR(100)")
                        .HasColumnName("MESSAGE");

                    b.Property<long>("Size")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("SIZE");

                    b.Property<long?>("SourceId")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("SOURCE_ID");

                    b.Property<int>("Type")
                        .IsConcurrencyToken()
                        .HasColumnType("INT")
                        .HasColumnName("TYPE");

                    b.HasKey("Uid");

                    b.HasIndex("DtCreated");

                    b.HasIndex("Id");

                    b.HasIndex("Message");

                    b.HasIndex("Size");

                    b.HasIndex("SourceId");

                    b.HasIndex("Type");

                    b.HasIndex("Uid")
                        .IsUnique();

                    b.ToTable("MESSAGES", (string)null);
                });

            modelBuilder.Entity("TgStorage.Domain.Proxies.TgEfProxyEntity", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("UID");

                    b.Property<string>("HostName")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("HOST_NAME");

                    b.Property<string>("Password")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("PASSWORD");

                    b.Property<ushort>("Port")
                        .IsConcurrencyToken()
                        .HasColumnType("INT(5)")
                        .HasColumnName("PORT");

                    b.Property<string>("Secret")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("SECRET");

                    b.Property<int>("Type")
                        .IsConcurrencyToken()
                        .HasColumnType("INT")
                        .HasColumnName("TYPE");

                    b.Property<string>("UserName")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("USER_NAME");

                    b.HasKey("Uid");

                    b.HasIndex("HostName");

                    b.HasIndex("Password");

                    b.HasIndex("Port");

                    b.HasIndex("Secret");

                    b.HasIndex("Type");

                    b.HasIndex("Uid")
                        .IsUnique();

                    b.HasIndex("UserName");

                    b.ToTable("PROXIES", (string)null);
                });

            modelBuilder.Entity("TgStorage.Domain.Sources.TgEfSourceEntity", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("UID");

                    b.Property<string>("About")
                        .IsConcurrencyToken()
                        .HasMaxLength(1024)
                        .HasColumnType("NVARCHAR(1024)")
                        .HasColumnName("ABOUT");

                    b.Property<long>("AccessHash")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("ACCESS_HASH");

                    b.Property<int>("Count")
                        .IsConcurrencyToken()
                        .HasColumnType("INT")
                        .HasColumnName("COUNT");

                    b.Property<string>("Directory")
                        .IsConcurrencyToken()
                        .HasMaxLength(256)
                        .HasColumnType("NVARCHAR(256)")
                        .HasColumnName("DIRECTORY");

                    b.Property<DateTime>("DtChanged")
                        .IsConcurrencyToken()
                        .HasColumnType("DATETIME")
                        .HasColumnName("DT_CHANGED");

                    b.Property<int>("FirstId")
                        .IsConcurrencyToken()
                        .HasColumnType("INT")
                        .HasColumnName("FIRST_ID");

                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("ID");

                    b.Property<bool>("IsActive")
                        .IsConcurrencyToken()
                        .HasColumnType("BIT")
                        .HasColumnName("IS_ACTIVE");

                    b.Property<bool>("IsAutoUpdate")
                        .IsConcurrencyToken()
                        .HasColumnType("BIT")
                        .HasColumnName("IS_AUTO_UPDATE");

                    b.Property<string>("Title")
                        .IsConcurrencyToken()
                        .HasMaxLength(256)
                        .HasColumnType("NVARCHAR(256)")
                        .HasColumnName("TITLE");

                    b.Property<string>("UserName")
                        .IsConcurrencyToken()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("USER_NAME");

                    b.HasKey("Uid");

                    b.HasIndex("AccessHash");

                    b.HasIndex("Count");

                    b.HasIndex("Directory");

                    b.HasIndex("DtChanged");

                    b.HasIndex("FirstId");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("IsActive");

                    b.HasIndex("IsAutoUpdate");

                    b.HasIndex("Title");

                    b.HasIndex("Uid")
                        .IsUnique();

                    b.HasIndex("UserName");

                    b.ToTable("SOURCES", (string)null);
                });

            modelBuilder.Entity("TgStorage.Domain.Stories.TgEfStoryEntity", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("UID");

                    b.Property<string>("Caption")
                        .IsConcurrencyToken()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("CAPTION");

                    b.Property<DateTime?>("Date")
                        .IsConcurrencyToken()
                        .HasColumnType("DATETIME")
                        .HasColumnName("DATE");

                    b.Property<DateTime>("DtChanged")
                        .IsConcurrencyToken()
                        .HasColumnType("DATETIME")
                        .HasColumnName("DT_CHANGED");

                    b.Property<DateTime?>("ExpireDate")
                        .IsConcurrencyToken()
                        .HasColumnType("DATETIME")
                        .HasColumnName("EXPIRE_DATE");

                    b.Property<long?>("FromId")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("FROM_ID");

                    b.Property<string>("FromName")
                        .IsConcurrencyToken()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("FROM_NAME");

                    b.Property<long>("Id")
                        .IsConcurrencyToken()
                        .HasColumnType("LONG(20)")
                        .HasColumnName("ID");

                    b.Property<int>("Length")
                        .IsConcurrencyToken()
                        .HasColumnType("INT(20)")
                        .HasColumnName("LENGTH");

                    b.Property<string>("Message")
                        .IsConcurrencyToken()
                        .HasMaxLength(256)
                        .HasColumnType("NVARCHAR(256)")
                        .HasColumnName("MESSAGE");

                    b.Property<int>("Offset")
                        .IsConcurrencyToken()
                        .HasColumnType("INT(20)")
                        .HasColumnName("OFFSET");

                    b.Property<string>("Type")
                        .IsConcurrencyToken()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("TYPE");

                    b.HasKey("Uid");

                    b.HasIndex("Caption");

                    b.HasIndex("Date");

                    b.HasIndex("DtChanged");

                    b.HasIndex("ExpireDate");

                    b.HasIndex("FromId");

                    b.HasIndex("FromName");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("Type");

                    b.HasIndex("Uid")
                        .IsUnique();

                    b.ToTable("STORIES", (string)null);
                });

            modelBuilder.Entity("TgStorage.Domain.Versions.TgEfVersionEntity", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("CHAR(36)")
                        .HasColumnName("UID");

                    b.Property<string>("Description")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("NVARCHAR(128)")
                        .HasColumnName("DESCRIPTION");

                    b.Property<short>("Version")
                        .IsConcurrencyToken()
                        .HasMaxLength(4)
                        .HasColumnType("SMALLINT")
                        .HasColumnName("VERSION");

                    b.HasKey("Uid");

                    b.HasIndex("Description");

                    b.HasIndex("Uid")
                        .IsUnique();

                    b.HasIndex("Version")
                        .IsUnique();

                    b.ToTable("VERSIONS", (string)null);
                });

            modelBuilder.Entity("TgStorage.Domain.Apps.TgEfAppEntity", b =>
                {
                    b.HasOne("TgStorage.Domain.Proxies.TgEfProxyEntity", "Proxy")
                        .WithMany("Apps")
                        .HasForeignKey("ProxyUid");

                    b.Navigation("Proxy");
                });

            modelBuilder.Entity("TgStorage.Domain.Documents.TgEfDocumentEntity", b =>
                {
                    b.HasOne("TgStorage.Domain.Sources.TgEfSourceEntity", "Source")
                        .WithMany("Documents")
                        .HasForeignKey("SourceId")
                        .HasPrincipalKey("Id");

                    b.Navigation("Source");
                });

            modelBuilder.Entity("TgStorage.Domain.Messages.TgEfMessageEntity", b =>
                {
                    b.HasOne("TgStorage.Domain.Sources.TgEfSourceEntity", "Source")
                        .WithMany("Messages")
                        .HasForeignKey("SourceId")
                        .HasPrincipalKey("Id");

                    b.Navigation("Source");
                });

            modelBuilder.Entity("TgStorage.Domain.Proxies.TgEfProxyEntity", b =>
                {
                    b.Navigation("Apps");
                });

            modelBuilder.Entity("TgStorage.Domain.Sources.TgEfSourceEntity", b =>
                {
                    b.Navigation("Documents");

                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
