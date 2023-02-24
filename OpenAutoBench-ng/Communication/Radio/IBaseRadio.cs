using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;

namespace OpenAutoBench_ng.Communication.Radio
{
    public interface IBaseRadio
    {
        public string Name { get; }

        public string SerialNumber { get; }

        public string FirmwareVersion { get; }

        public string InfoHeader { get; }
    }
}
