using TodoApp.Features.Lists;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Lists;

public class ListArchiveHandlerTests
{
    [Fact]
    public async Task Archive_NonDefaultList_IsHiddenFromGetLists()
    {
        var db = await TestDatabase.CreateAsync();
        var create = new CreateListHandler(db);
        var archive = new ArchiveListHandler(db);
        var get = new GetListsHandler(db);

        var workId = await create.HandleAsync("Work");
        await archive.HandleAsync(workId);

        var lists = await get.HandleAsync();
        Assert.DoesNotContain(lists, l => l.Id == workId);
    }

    [Fact]
    public async Task Archive_DefaultList_ThrowsInvalidOperation()
    {
        var db = await TestDatabase.CreateAsync();
        var archive = new ArchiveListHandler(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            archive.HandleAsync(ArchiveListHandler.DefaultListId));
    }

    [Fact]
    public async Task GetArchivedLists_ReturnsArchivedListsOnly()
    {
        var db = await TestDatabase.CreateAsync();
        var create = new CreateListHandler(db);
        var archive = new ArchiveListHandler(db);
        var getArchived = new GetArchivedListsHandler(db);

        var workId = await create.HandleAsync("Work");
        await create.HandleAsync("Personal alt");
        await archive.HandleAsync(workId);

        var archived = await getArchived.HandleAsync();
        Assert.Single(archived);
        Assert.Equal("Work", archived[0].Name);
        Assert.True(archived[0].IsArchived);
    }

    [Fact]
    public async Task GetArchivedLists_WhenNoneArchived_ReturnsEmpty()
    {
        var db = await TestDatabase.CreateAsync();
        var getArchived = new GetArchivedListsHandler(db);

        var archived = await getArchived.HandleAsync();
        Assert.Empty(archived);
    }

    [Fact]
    public async Task Unarchive_RestoresListToGetLists()
    {
        var db = await TestDatabase.CreateAsync();
        var create = new CreateListHandler(db);
        var archive = new ArchiveListHandler(db);
        var unarchive = new UnarchiveListHandler(db);
        var get = new GetListsHandler(db);
        var getArchived = new GetArchivedListsHandler(db);

        var workId = await create.HandleAsync("Work");
        await archive.HandleAsync(workId);

        // Confirm archived
        var archived = await getArchived.HandleAsync();
        Assert.Single(archived);

        await unarchive.HandleAsync(workId);

        var lists = await get.HandleAsync();
        Assert.Contains(lists, l => l.Id == workId && l.Name == "Work");
        var archivedAfter = await getArchived.HandleAsync();
        Assert.Empty(archivedAfter);
    }

    [Fact]
    public async Task Archive_MultipleListsArchivedInSequence()
    {
        var db = await TestDatabase.CreateAsync();
        var create = new CreateListHandler(db);
        var archive = new ArchiveListHandler(db);
        var get = new GetListsHandler(db);
        var getArchived = new GetArchivedListsHandler(db);

        var id1 = await create.HandleAsync("Project A");
        var id2 = await create.HandleAsync("Project B");
        await archive.HandleAsync(id1);
        await archive.HandleAsync(id2);

        var active = await get.HandleAsync();
        Assert.DoesNotContain(active, l => l.Id == id1);
        Assert.DoesNotContain(active, l => l.Id == id2);

        var archived = await getArchived.HandleAsync();
        Assert.Equal(2, archived.Count);
    }

    [Fact]
    public async Task Archive_DefaultPersonalList_RemainsInGetLists()
    {
        var db = await TestDatabase.CreateAsync();
        var create = new CreateListHandler(db);
        var archive = new ArchiveListHandler(db);
        var get = new GetListsHandler(db);

        var workId = await create.HandleAsync("Work");
        await archive.HandleAsync(workId);

        var lists = await get.HandleAsync();
        Assert.Contains(lists, l => l.Id == 1 && l.Name == "Personal");
    }

    [Fact]
    public async Task ArchivedList_IsNotArchived_HasIsArchivedFalse()
    {
        var db = await TestDatabase.CreateAsync();
        var get = new GetListsHandler(db);

        var lists = await get.HandleAsync();
        Assert.All(lists, l => Assert.False(l.IsArchived));
    }

    [Fact]
    public async Task Archive_ThenUnarchive_ListRetainsName()
    {
        var db = await TestDatabase.CreateAsync();
        var create = new CreateListHandler(db);
        var archive = new ArchiveListHandler(db);
        var unarchive = new UnarchiveListHandler(db);
        var get = new GetListsHandler(db);

        var id = await create.HandleAsync("Sprint 1");
        await archive.HandleAsync(id);
        await unarchive.HandleAsync(id);

        var lists = await get.HandleAsync();
        Assert.Contains(lists, l => l.Id == id && l.Name == "Sprint 1");
    }
}
