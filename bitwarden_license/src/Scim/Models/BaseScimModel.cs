﻿namespace Bit.Scim.Models;

public abstract class BaseScimModel
{
    protected BaseScimModel()
    { }

    public BaseScimModel(string schema)
    {
        Schemas = new List<string> { schema };
    }

    public List<string> Schemas { get; set; }
}
