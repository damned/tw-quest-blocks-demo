public interface Latch
{
    Latch Apply();
    void Destroy();
    LatchEnd OtherEndTo(LatchEnd refLatchEnd);
}