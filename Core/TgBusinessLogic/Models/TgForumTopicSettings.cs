using TL;

namespace TgBusinessLogic.Models;

/// <summary> Forum topic settings </summary>
public sealed class TgForumTopicSettings
{
    #region Fields, properties, constructor

    /// <summary> Root topic of the forum, usually with ID = 1 </summary>
    public ForumTopicBase? RootTopic { get; set; }
    /// <summary> All topics of the forum </summary>
    public List<ForumTopicBase> Topics { get; set; } = [];

    #endregion

    #region Methods

    /// <summary> Set topics from the forum topics response </summary>
    public void SetTopics(Messages_ForumTopics forumTopics)
    {
        Topics = [.. forumTopics.topics];
        RootTopic = Topics.SingleOrDefault(x => x.ID == 1);
    }

    #endregion
}
