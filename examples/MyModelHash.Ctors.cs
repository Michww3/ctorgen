public MyModelHash(
    IGuid id,
    IString name,
    DateTime timestamp
)
    : this(
        new DeterminedHash(id),
        name,
        timestamp
    )
    { }

public MyModelHash(
    IDeterminedHash idHash,
    IString name,
    DateTime timestamp
)
    : this(
        idHash,
        new DeterminedHash(name),
        timestamp
    )
    { }

public MyModelHash(
    IGuid id,
    IDeterminedHash nameHash,
    DateTime timestamp
)
    : this(
        new DeterminedHash(id),
        nameHash,
        timestamp
    )
    { }

public MyModelHash(
    IGuid id,
    IString name,
    IDeterminedHash timestampHash
)
    : this(
        new DeterminedHash(id),
        name,
        timestampHash
    )
    { }

public MyModelHash(
    IDeterminedHash idHash,
    IDeterminedHash nameHash,
    DateTime timestamp
)
    : this(
        idHash,
        nameHash,
        new DeterminedHash(timestamp)
    )
    { }

public MyModelHash(
    IDeterminedHash idHash,
    IString name,
    IDeterminedHash timestampHash
)
    : this(
        idHash,
        new DeterminedHash(name),
        timestampHash
    )
    { }

public MyModelHash(
    IGuid id,
    IDeterminedHash nameHash,
    IDeterminedHash timestampHash
)
    : this(
        new DeterminedHash(id),
        nameHash,
        timestampHash
    )
    { }

public MyModelHash(
    IDeterminedHash idHash,
    IDeterminedHash nameHash,
    IDeterminedHash timestampHash
)
    {
    _idHash = idHash;
    _nameHash = nameHash;
    _timestampHash = timestampHash;
    }
