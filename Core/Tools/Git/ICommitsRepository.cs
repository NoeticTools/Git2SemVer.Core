﻿namespace NoeticTools.Git2SemVer.Core.Tools.Git;

public interface ICommitsRepository
{
    bool TryGet(CommitId commitId, out Commit commit);
    bool TryGet(string commitSha, out Commit commit1);
    void Add(params Commit[] commits);
    void Add(IReadOnlyList<Commit> commits);
}