using System;

public interface LatchBinder
{
    public void Apply(LatchEnd fromEnd, LatchEnd toEnd);
    void Destroy();
}
