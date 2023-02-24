namespace OpenAutoBench_ng.Communication.Radio
{
    public interface IBaseTest
    {
        public string name { get; }

        public bool pass { get; }
        public bool testCompleted { get; }

        public bool isRadioEligible();

        public Task setup();

        public Task performTest();

        public Task performAlignment();

        public Task teardown();

        
    }
}
