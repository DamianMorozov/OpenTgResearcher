// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

namespace TgBusinessLogic.Models;

/// <summary> Forum topic settings </summary>
public sealed class TgForumTopicSettings
{
    #region Public and private fields, properties, constructor

    /// <summary> Root topic of the forum, usually with ID = 1 </summary>
    public ForumTopicBase? RootTopic { get; set; }
    /// <summary> All topics of the forum </summary>
    public List<ForumTopicBase> Topics { get; set; } = [];

    #endregion

    #region Public and private methods

    /// <summary> Set topics from the forum topics response </summary>
    public void SetTopics(Messages_ForumTopics forumTopics)
    {
        Topics = [.. forumTopics.topics];
        RootTopic = Topics.SingleOrDefault(x => x.ID == 1);
    }

    #endregion
}
