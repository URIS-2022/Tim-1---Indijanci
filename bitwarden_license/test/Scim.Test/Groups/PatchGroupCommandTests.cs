﻿using System.Text.Json;
using Bit.Core.Entities;
using Bit.Core.Exceptions;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Scim.Groups;
using Bit.Scim.Models;
using Bit.Scim.Utilities;
using Bit.Test.Common.AutoFixture;
using Bit.Test.Common.AutoFixture.Attributes;
using NSubstitute;
using Xunit;

namespace Bit.Scim.Test.Groups;

[SutProviderCustomize]
public class PatchGroupCommandTests
{
    [Theory]
    [BitAutoData]
    public async Task PatchGroup_ReplaceListMembers_Success(SutProvider<PatchGroupCommand> sutProvider, Group group, IEnumerable<Guid> userIds)
    {
        sutProvider.GetDependency<IGroupRepository>()
            .GetByIdAsync(group.Id)
            .Returns(group);

        var scimPatchModel = new Models.ScimPatchModel
        {
            Operations = new List<ScimPatchModel.OperationModel>
            {
                new ScimPatchModel.OperationModel
                {
                    Op = "replace",
                    Path = "members",
                    Value = JsonDocument.Parse(JsonSerializer.Serialize(userIds.Select(uid => new { value = uid }).ToArray())).RootElement
                }
            },
            Schemas = new List<string> { ScimConstants.Scim2SchemaUser }
        };

        await sutProvider.Sut.PatchGroupAsync(group.OrganizationId, group.Id, scimPatchModel);

        await sutProvider.GetDependency<IGroupRepository>().Received(1).UpdateUsersAsync(group.Id, Arg.Is<IEnumerable<Guid>>(arg => arg.All(id => userIds.Contains(id))));
    }

    [Theory]
    [BitAutoData]
    public async Task PatchGroup_ReplaceDisplayNameFromPath_Success(SutProvider<PatchGroupCommand> sutProvider, Group group, string displayName)
    {
        sutProvider.GetDependency<IGroupRepository>()
            .GetByIdAsync(group.Id)
            .Returns(group);

        var scimPatchModel = new Models.ScimPatchModel
        {
            Operations = new List<ScimPatchModel.OperationModel>
            {
                new ScimPatchModel.OperationModel
                {
                    Op = "replace",
                    Path = "displayname",
                    Value = JsonDocument.Parse($"\"{displayName}\"").RootElement
                }
            },
            Schemas = new List<string> { ScimConstants.Scim2SchemaUser }
        };

        await sutProvider.Sut.PatchGroupAsync(group.OrganizationId, group.Id, scimPatchModel);

        await sutProvider.GetDependency<IGroupService>().Received(1).SaveAsync(group);
        Assert.Equal(displayName, group.Name);
    }

    [Theory]
    [BitAutoData]
    public async Task PatchGroup_ReplaceDisplayNameFromValueObject_Success(SutProvider<PatchGroupCommand> sutProvider, Group group, string displayName)
    {
        sutProvider.GetDependency<IGroupRepository>()
            .GetByIdAsync(group.Id)
            .Returns(group);

        var scimPatchModel = new Models.ScimPatchModel
        {
            Operations = new List<ScimPatchModel.OperationModel>
            {
                new ScimPatchModel.OperationModel
                {
                    Op = "replace",
                    Value = JsonDocument.Parse($"{{\"displayName\":\"{displayName}\"}}").RootElement
                }
            },
            Schemas = new List<string> { ScimConstants.Scim2SchemaUser }
        };

        await sutProvider.Sut.PatchGroupAsync(group.OrganizationId, group.Id, scimPatchModel);

        await sutProvider.GetDependency<IGroupService>().Received(1).SaveAsync(group);
        Assert.Equal(displayName, group.Name);
    }

    [Theory]
    [BitAutoData]
    public async Task PatchGroup_AddSingleMember_Success(SutProvider<PatchGroupCommand> sutProvider, Group group, ICollection<Guid> existingMembers, Guid userId)
    {
        sutProvider.GetDependency<IGroupRepository>()
            .GetByIdAsync(group.Id)
            .Returns(group);

        sutProvider.GetDependency<IGroupRepository>()
            .GetManyUserIdsByIdAsync(group.Id)
            .Returns(existingMembers);

        var scimPatchModel = new Models.ScimPatchModel
        {
            Operations = new List<ScimPatchModel.OperationModel>
            {
                new ScimPatchModel.OperationModel
                {
                    Op = "add",
                    Path = $"members[value eq \"{userId}\"]",
                }
            },
            Schemas = new List<string> { ScimConstants.Scim2SchemaUser }
        };

        await sutProvider.Sut.PatchGroupAsync(group.OrganizationId, group.Id, scimPatchModel);

        await sutProvider.GetDependency<IGroupRepository>().Received(1).UpdateUsersAsync(group.Id, Arg.Is<IEnumerable<Guid>>(arg => arg.All(id => existingMembers.Append(userId).Contains(id))));
    }

    [Theory]
    [BitAutoData]
    public async Task PatchGroup_AddListMembers_Success(SutProvider<PatchGroupCommand> sutProvider, Group group, ICollection<Guid> existingMembers, ICollection<Guid> userIds)
    {
        sutProvider.GetDependency<IGroupRepository>()
            .GetByIdAsync(group.Id)
            .Returns(group);

        sutProvider.GetDependency<IGroupRepository>()
            .GetManyUserIdsByIdAsync(group.Id)
            .Returns(existingMembers);

        var scimPatchModel = new Models.ScimPatchModel
        {
            Operations = new List<ScimPatchModel.OperationModel>
            {
                new ScimPatchModel.OperationModel
                {
                    Op = "add",
                    Path = $"members",
                    Value = JsonDocument.Parse(JsonSerializer.Serialize(userIds.Select(uid => new { value = uid }).ToArray())).RootElement
                }
            },
            Schemas = new List<string> { ScimConstants.Scim2SchemaUser }
        };

        await sutProvider.Sut.PatchGroupAsync(group.OrganizationId, group.Id, scimPatchModel);

        await sutProvider.GetDependency<IGroupRepository>().Received(1).UpdateUsersAsync(group.Id, Arg.Is<IEnumerable<Guid>>(arg => arg.All(id => existingMembers.Concat(userIds).Contains(id))));
    }

    [Theory]
    [BitAutoData]
    public async Task PatchGroup_RemoveSingleMember_Success(SutProvider<PatchGroupCommand> sutProvider, Group group, Guid userId)
    {
        sutProvider.GetDependency<IGroupRepository>()
            .GetByIdAsync(group.Id)
            .Returns(group);

        var scimPatchModel = new Models.ScimPatchModel
        {
            Operations = new List<ScimPatchModel.OperationModel>
            {
                new ScimPatchModel.OperationModel
                {
                    Op = "remove",
                    Path = $"members[value eq \"{userId}\"]",
                }
            },
            Schemas = new List<string> { ScimConstants.Scim2SchemaUser }
        };

        await sutProvider.Sut.PatchGroupAsync(group.OrganizationId, group.Id, scimPatchModel);

        await sutProvider.GetDependency<IGroupService>().Received(1).DeleteUserAsync(group, userId);
    }

    [Theory]
    [BitAutoData]
    public async Task PatchGroup_RemoveListMembers_Success(SutProvider<PatchGroupCommand> sutProvider, Group group, ICollection<Guid> existingMembers)
    {
        sutProvider.GetDependency<IGroupRepository>()
            .GetByIdAsync(group.Id)
            .Returns(group);

        sutProvider.GetDependency<IGroupRepository>()
            .GetManyUserIdsByIdAsync(group.Id)
            .Returns(existingMembers);

        var scimPatchModel = new Models.ScimPatchModel
        {
            Operations = new List<ScimPatchModel.OperationModel>
            {
                new ScimPatchModel.OperationModel
                {
                    Op = "remove",
                    Path = $"members",
                    Value = JsonDocument.Parse(JsonSerializer.Serialize(existingMembers.Select(uid => new { value = uid }).ToArray())).RootElement
                }
            },
            Schemas = new List<string> { ScimConstants.Scim2SchemaUser }
        };

        await sutProvider.Sut.PatchGroupAsync(group.OrganizationId, group.Id, scimPatchModel);

        await sutProvider.GetDependency<IGroupRepository>().Received(1).UpdateUsersAsync(group.Id, Arg.Is<IEnumerable<Guid>>(arg => arg.All(id => existingMembers.Contains(id))));
    }

    [Theory]
    [BitAutoData]
    public async Task PatchGroup_NoAction_Success(SutProvider<PatchGroupCommand> sutProvider, Group group)
    {
        sutProvider.GetDependency<IGroupRepository>()
            .GetByIdAsync(group.Id)
            .Returns(group);

        var scimPatchModel = new Models.ScimPatchModel
        {
            Operations = new List<ScimPatchModel.OperationModel>(),
            Schemas = new List<string> { ScimConstants.Scim2SchemaUser }
        };

        await sutProvider.Sut.PatchGroupAsync(group.OrganizationId, group.Id, scimPatchModel);

        await sutProvider.GetDependency<IGroupRepository>().Received(0).UpdateUsersAsync(group.Id, Arg.Any<IEnumerable<Guid>>());
        await sutProvider.GetDependency<IGroupRepository>().Received(0).GetManyUserIdsByIdAsync(group.Id);
        await sutProvider.GetDependency<IGroupService>().Received(0).SaveAsync(group);
        await sutProvider.GetDependency<IGroupService>().Received(0).DeleteUserAsync(group, Arg.Any<Guid>());
    }

    [Theory]
    [BitAutoData]
    public async Task PatchGroup_NotFound_Throws(SutProvider<PatchGroupCommand> sutProvider, Guid organizationId, Guid groupId)
    {
        var scimPatchModel = new Models.ScimPatchModel
        {
            Operations = new List<ScimPatchModel.OperationModel>(),
            Schemas = new List<string> { ScimConstants.Scim2SchemaUser }
        };

        await Assert.ThrowsAsync<NotFoundException>(async () => await sutProvider.Sut.PatchGroupAsync(organizationId, groupId, scimPatchModel));
    }

    [Theory]
    [BitAutoData]
    public async Task PatchGroup_MismatchingOrganizationId_Throws(SutProvider<PatchGroupCommand> sutProvider, Guid organizationId, Guid groupId)
    {
        var scimPatchModel = new Models.ScimPatchModel
        {
            Operations = new List<ScimPatchModel.OperationModel>(),
            Schemas = new List<string> { ScimConstants.Scim2SchemaUser }
        };

        sutProvider.GetDependency<IGroupRepository>()
            .GetByIdAsync(groupId)
            .Returns(new Group
            {
                Id = groupId,
                OrganizationId = Guid.NewGuid()
            });

        await Assert.ThrowsAsync<NotFoundException>(async () => await sutProvider.Sut.PatchGroupAsync(organizationId, groupId, scimPatchModel));
    }
}
