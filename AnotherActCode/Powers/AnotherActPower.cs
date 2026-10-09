using AnotherAct.AnotherActCode.Extensions;
using BaseLib.Abstracts;
using BaseLib.Extensions;

namespace AnotherAct.AnotherActCode.Powers;

public abstract class AnotherActPower : CustomPowerModel
{
    //Loads from AnotherAct/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}