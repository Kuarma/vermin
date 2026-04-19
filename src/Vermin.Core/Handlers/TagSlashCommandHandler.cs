using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Octokit;

namespace Vermin.Handlers;

//BUG: channelName must follow discord's naming convention (regex or typeCon)
[Group(
        name: "tag",
        description: "Configure or create a tag")]
public class TagSlashCommandHandler(
        ILogger<TagSlashCommandHandler> logger,
        GitHubClient gitHubClient) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<TagSlashCommandHandler> _logger = logger;
    private readonly GitHubClient _gitHubClient = gitHubClient;

    [SlashCommand(
            name: "new",
            description: "Create a new tag")]
    public async Task CreateTagAsync(
            string repositoryName,
            string repositoryHost,
            [ChannelTypes(ChannelType.Forum)] IForumChannel forum,
            bool isModerated = true)
    {
        var tag = new ForumTagBuilder(
                name: $"{repositoryName}",
                isModerated: isModerated,
                emoji: null)
            .Build();

        await forum.ModifyAsync(properties => properties
                .Tags = new[] { tag });

        //Only for testing
        var issuesForOctokit = await _gitHubClient
            .Issue
            .GetAllForRepository(
                    owner: repositoryHost,
                    name: repositoryName);

        await DeferAsync(ephemeral: false);

        foreach (var issue in issuesForOctokit)
        {
            await forum.CreatePostAsync(
                    title: issue.Title,
                    archiveDuration: ThreadArchiveDuration.OneWeek,
                    text: "test");

            await FollowupAsync(
                    $"Created thread for: {issue.Title} in {forum.Name}");
        }
    }

    [SlashCommand(
            name: "edit",
            description: "Edit a tag")]
    public async Task EditTagAsync(
            string tagName)
    {
    }

    [SlashCommand(
            name: "delete",
            description: "Delete a tag")]
    public async Task DeleteTagAsync(
            string tagName)
    {
    }
}