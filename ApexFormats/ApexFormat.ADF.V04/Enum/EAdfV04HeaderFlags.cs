﻿namespace ApexFormat.ADF.V04.Enum;

[Flags]
public enum EAdfV04HeaderFlags : uint
{
    Default                        = 0x0,
    RelativeOffsetExists           = 0x1,
    ContainsInlineArrayWithPointer = 0x2,
}
